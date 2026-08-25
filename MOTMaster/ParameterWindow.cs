using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using DAQ.Environment;

namespace MOTMaster
{
    public partial class ParameterWindow : Form
    {
        private readonly string _parameterFolder = (string)Environs.FileSystem.Paths["scriptListPath"];

        // Supported parameter types shown in the Type column dropdown
        private readonly Type[] _parameterTypes =
        {
            typeof(int), typeof(double)
        };

        private string _currentFilePath = null;

        // Inline subitem editor state (ListView has no built-in per-cell editing)
        private Control _activeEditor = null;
        private ListViewItem _editingItem = null;
        private int _editingSubItemIndex = -1;
        private bool _isClosingEditor = false;

        // Right-click context menu (built dynamically - "Move to Group" lists whatever groups currently exist)
        private readonly ContextMenuStrip _itemContextMenu = new ContextMenuStrip();

        // ---------------------------------------------------------------
        public ParameterWindow()
        {
            InitializeComponent();

            _itemContextMenu.Opening += ItemContextMenu_Opening;
            lvParameters.ContextMenuStrip = _itemContextMenu;
        }

        // ================================================================
        // COLLAPSIBLE GROUP HEADERS
        //
        // .NET Framework's ListViewGroup has no managed API for this (the
        // CollapsedState property was only added in .NET 5+); the underlying
        // Win32 common control has supported it since Vista via LVM_SETGROUPINFO.
        // Once a group's LVGS_COLLAPSIBLE state bit is set, comctl32 draws the
        // chevron and handles header-click collapse/expand entirely natively -
        // no further managed-side wiring needed, and the ListView's Items
        // collection is unaffected (collapsing is purely visual).
        //
        // This needs the group's native Win32 group ID, which ListViewGroup
        // only exposes as an internal "ID" property - fetched via reflection.
        // Best-effort: if that internal shape ever changes, groups just stay
        // non-collapsible rather than the window breaking.
        // ================================================================
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref LVGROUP lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LVGROUP
        {
            public int cbSize;
            public int mask;
            public IntPtr pszHeader;
            public int cchHeader;
            public IntPtr pszFooter;
            public int cchFooter;
            public int iGroupId;
            public int stateMask;
            public int state;
            public int uAlign;
            public IntPtr pszSubtitle;
            public int cchSubtitle;
            public IntPtr pszTask;
            public int cchTask;
            public IntPtr pszDescriptionTop;
            public int cchDescriptionTop;
            public IntPtr pszDescriptionBottom;
            public int cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public int cItems;
            public IntPtr pszSubsetTitle;
            public int cchSubsetTitle;
        }

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETGROUPINFO = LVM_FIRST + 147;
        private const int LVGF_STATE = 0x0004;
        private const int LVGS_COLLAPSIBLE = 0x0008;

        private ListViewGroup CreateGroup(string name)
        {
            var group = new ListViewGroup(name);
            lvParameters.Groups.Add(group);
            ScheduleMakeGroupCollapsible(group);
            return group;
        }

        // ListViewGroup.ID is assigned the moment the group is constructed, but the
        // *native* Win32 group (which LVM_SETGROUPINFO needs to already exist) isn't
        // necessarily registered yet at this point - e.g. RebuildListView adds every
        // group inside a BeginUpdate/EndUpdate block. BeginInvoke defers the actual
        // SendMessage call to after the current update cycle has fully drained, so the
        // native group is guaranteed to exist by the time it runs.
        private void ScheduleMakeGroupCollapsible(ListViewGroup group)
        {
            if (!lvParameters.IsHandleCreated) return;
            lvParameters.BeginInvoke((MethodInvoker)(() => MakeGroupCollapsible(group)));
        }

        private void MakeGroupCollapsible(ListViewGroup group)
        {
            try
            {
                int groupId = GetNativeGroupId(group);

                var info = new LVGROUP
                {
                    cbSize = Marshal.SizeOf(typeof(LVGROUP)),
                    mask = LVGF_STATE,
                    iGroupId = groupId,
                    stateMask = LVGS_COLLAPSIBLE,
                    state = LVGS_COLLAPSIBLE
                };

                IntPtr result = SendMessage(lvParameters.Handle, LVM_SETGROUPINFO, (IntPtr)groupId, ref info);
                if (result == (IntPtr)(-1))
                    Debug.WriteLine($"ParameterWindow: LVM_SETGROUPINFO failed for group \"{group.Header}\" (id {groupId}) - header stays non-collapsible.");
            }
            catch (Exception ex)
            {
                // See class-level remarks above - non-collapsible groups are an
                // acceptable degradation, a broken window is not.
                Debug.WriteLine($"ParameterWindow: could not make group \"{group.Header}\" collapsible: {ex}");
            }
        }

        private static int GetNativeGroupId(ListViewGroup group)
        {
            var idProperty = typeof(ListViewGroup).GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? typeof(ListViewGroup).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty != null)
                return (int)idProperty.GetValue(group, null);

            var idField = typeof(ListViewGroup).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?? typeof(ListViewGroup).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            if (idField != null)
                return (int)idField.GetValue(group);

            throw new InvalidOperationException("ListViewGroup has no recognisable native-ID member.");
        }

        // ================================================================
        // FORM LOAD
        // ================================================================
        private void ParameterWindow_Load(object sender, EventArgs e)
        {
            // Ensure the folder exists
            if (!Directory.Exists(_parameterFolder))
            {
                Directory.CreateDirectory(_parameterFolder);
                MessageBox.Show(
                    $"Parameter folder created at:\n{_parameterFolder}\n\nAdd .json files to this folder.",
                    "Folder Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            lblFolder.Text = $"Folder: {_parameterFolder}";
            LoadFileList();
        }

        // ================================================================
        // FILE LIST
        // ================================================================
        private void LoadFileList()
        {
            cmbFiles.Items.Clear();

            var files = Directory.GetFiles(_parameterFolder, "*.json");
            foreach (var file in files)
                cmbFiles.Items.Add(Path.GetFileName(file));

            if (cmbFiles.Items.Count > 0)
                cmbFiles.SelectedIndex = 0;
            else
            {
                _currentFilePath = null;
                lvParameters.Items.Clear();
                lvParameters.Groups.Clear();
                RefreshTargetGroupCombo();
            }
        }

        private void cmbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFiles.SelectedItem == null) return;

            _currentFilePath = Path.Combine(_parameterFolder, cmbFiles.SelectedItem.ToString());
            LoadParametersFromFile();
        }

        // ================================================================
        // LOAD / SAVE
        // ================================================================
        private void LoadParametersFromFile()
        {
            var groups = ParameterFileManager.ReadFile(_currentFilePath, _parameterTypes[0]);
            RebuildListView(groups);
        }

        private void RebuildListView(List<ParameterGroup> groups)
        {
            lvParameters.BeginUpdate();
            lvParameters.Items.Clear();
            lvParameters.Groups.Clear();

            foreach (var group in groups)
            {
                var lvg = CreateGroup(group.Name);

                foreach (var entry in group.Parameters)
                {
                    var item = new ListViewItem(new[] { entry.Name, entry.Value, entry.Type.FullName });
                    item.Group = lvg;
                    lvParameters.Items.Add(item);
                }
            }

            lvParameters.EndUpdate();
            RefreshTargetGroupCombo();
        }

        private void SaveParametersToFile()
        {
            if (_currentFilePath == null) return;

            var groups = GatherGroupsFromListView();
            ParameterFileManager.WriteFile(_currentFilePath, groups);
        }

        private List<ParameterGroup> GatherGroupsFromListView()
        {
            var groups = new List<ParameterGroup>();
            var byGroup = new Dictionary<ListViewGroup, ParameterGroup>();

            foreach (ListViewGroup lvg in lvParameters.Groups)
            {
                var group = new ParameterGroup(lvg.Header);
                groups.Add(group);
                byGroup[lvg] = group;
            }

            foreach (ListViewItem item in lvParameters.Items)
            {
                if (item.Group == null) continue;
                ParameterGroup group;
                if (!byGroup.TryGetValue(item.Group, out group)) continue;

                string name = item.SubItems[0].Text;
                string value = item.SubItems.Count > 1 ? item.SubItems[1].Text : string.Empty;
                string typeStr = item.SubItems.Count > 2 ? item.SubItems[2].Text : string.Empty;

                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(value))
                    continue;

                Type resolvedType = Type.GetType(typeStr) ?? _parameterTypes[0];
                group.Parameters.Add(new ParameterEntry(name, value, resolvedType));
            }

            return groups;
        }

        // ================================================================
        // TARGET-GROUP COMBO (for Add Parameter, and to bootstrap dropping
        // into a still-empty group)
        // ================================================================
        private void RefreshTargetGroupCombo(ListViewGroup selectGroup = null)
        {
            cmbTargetGroup.Items.Clear();
            foreach (ListViewGroup lvg in lvParameters.Groups)
                cmbTargetGroup.Items.Add(lvg.Header);

            if (cmbTargetGroup.Items.Count == 0) return;

            int indexToSelect = selectGroup != null ? cmbTargetGroup.Items.IndexOf(selectGroup.Header) : -1;
            cmbTargetGroup.SelectedIndex = indexToSelect >= 0 ? indexToSelect : 0;
        }

        private ListViewGroup GetSelectedTargetGroup()
        {
            if (cmbTargetGroup.SelectedItem == null) return null;

            string header = cmbTargetGroup.SelectedItem.ToString();
            foreach (ListViewGroup lvg in lvParameters.Groups)
                if (lvg.Header == header)
                    return lvg;

            return null;
        }

        // ================================================================
        // INLINE CELL EDITING (ListView has no built-in per-cell editor)
        // ================================================================
        private void lvParameters_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hit = lvParameters.HitTest(e.Location);
            if (hit.Item == null) return;

            int subItemIndex = 0;
            for (int i = 0; i < hit.Item.SubItems.Count; i++)
            {
                if (hit.Item.SubItems[i] == hit.SubItem)
                {
                    subItemIndex = i;
                    break;
                }
            }

            BeginEditSubItem(hit.Item, subItemIndex);
        }

        private void BeginEditSubItem(ListViewItem item, int subItemIndex)
        {
            CommitEdit();
            _editingItem = item;
            _editingSubItemIndex = subItemIndex;

            Rectangle bounds = item.SubItems[subItemIndex].Bounds;

            if (subItemIndex == 2) // Type column -> ComboBox of the supported types
            {
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Bounds = bounds
                };
                foreach (var t in _parameterTypes) combo.Items.Add(t.FullName);
                combo.SelectedIndex = combo.Items.IndexOf(item.SubItems[subItemIndex].Text);
                if (combo.SelectedIndex < 0) combo.SelectedIndex = 0;
                combo.SelectedIndexChanged += (s, e) => CommitEdit();
                combo.Leave += (s, e) => CommitEdit();

                lvParameters.Controls.Add(combo);
                combo.Focus();
                combo.DroppedDown = true;
                _activeEditor = combo;
            }
            else
            {
                var textBox = new TextBox
                {
                    Bounds = bounds,
                    Text = item.SubItems[subItemIndex].Text,
                    BorderStyle = BorderStyle.FixedSingle
                };
                textBox.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter) { CommitEdit(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Escape) { CancelEdit(); e.Handled = true; }
                };
                textBox.Leave += (s, e) => CommitEdit();

                lvParameters.Controls.Add(textBox);
                textBox.Focus();
                textBox.SelectAll();
                _activeEditor = textBox;
            }
        }

        private void CommitEdit()
        {
            if (_activeEditor == null || _isClosingEditor) return;
            _isClosingEditor = true;

            var combo = _activeEditor as ComboBox;
            string newText = combo != null
                ? (combo.SelectedItem != null ? combo.SelectedItem.ToString() : string.Empty)
                : _activeEditor.Text;

            var item = _editingItem;
            var index = _editingSubItemIndex;

            CleanupEditor();

            if (item != null)
            {
                item.SubItems[index].Text = newText;
                SaveParametersToFile();
            }

            _isClosingEditor = false;
        }

        private void CancelEdit()
        {
            if (_isClosingEditor) return;
            _isClosingEditor = true;
            CleanupEditor();
            _isClosingEditor = false;
        }

        private void CleanupEditor()
        {
            if (_activeEditor != null)
            {
                lvParameters.Controls.Remove(_activeEditor);
                _activeEditor.Dispose();
                _activeEditor = null;
            }
            _editingItem = null;
            _editingSubItemIndex = -1;
        }

        // ================================================================
        // DRAG-AND-DROP GROUPING
        // ================================================================
        private void lvParameters_ItemDrag(object sender, ItemDragEventArgs e)
        {
            CommitEdit();
            lvParameters.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void lvParameters_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ListViewItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void lvParameters_DragDrop(object sender, DragEventArgs e)
        {
            var draggedItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
            if (draggedItem == null) return;

            Point clientPoint = lvParameters.PointToClient(new Point(e.X, e.Y));
            var hit = lvParameters.HitTest(clientPoint);

            // ListView's HitTest doesn't resolve a group directly - only via an item
            // already in it, so an item must exist in the target group to drop onto.
            // (A brand-new empty group is populated via the "Add to group" combo below,
            // after which it has an item and becomes a valid drop target too.)
            if (hit.Item == null || hit.Item.Group == null) return;
            if (hit.Item.Group == draggedItem.Group) return;

            draggedItem.Group = hit.Item.Group;
            SaveParametersToFile();
        }

        // ================================================================
        // RIGHT-CLICK CONTEXT MENU: Move to Group / Delete
        // ================================================================
        private void lvParameters_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            CommitEdit();

            var hit = lvParameters.HitTest(e.Location);
            if (hit.Item == null)
            {
                lvParameters.SelectedItems.Clear();
                return;
            }

            if (!hit.Item.Selected)
            {
                lvParameters.SelectedItems.Clear();
                hit.Item.Selected = true;
            }
            hit.Item.Focused = true;
        }

        private void ItemContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (lvParameters.SelectedItems.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            _itemContextMenu.Items.Clear();

            var moveToGroup = new ToolStripMenuItem("Move to Group");
            foreach (ListViewGroup lvg in lvParameters.Groups)
            {
                var groupItem = new ToolStripMenuItem(lvg.Header);
                groupItem.Click += (s, args) => MoveSelectedItemsToGroup(lvg);
                moveToGroup.DropDownItems.Add(groupItem);
            }
            moveToGroup.Enabled = moveToGroup.DropDownItems.Count > 0;
            _itemContextMenu.Items.Add(moveToGroup);

            _itemContextMenu.Items.Add(new ToolStripSeparator());

            var delete = new ToolStripMenuItem("Delete");
            delete.Click += (s, args) => DeleteSelectedRows();
            _itemContextMenu.Items.Add(delete);
        }

        private void MoveSelectedItemsToGroup(ListViewGroup targetGroup)
        {
            var items = new List<ListViewItem>();
            foreach (ListViewItem item in lvParameters.SelectedItems) items.Add(item);

            bool changed = false;
            foreach (var item in items)
            {
                if (item.Group != targetGroup)
                {
                    item.Group = targetGroup;
                    changed = true;
                }
            }

            if (changed) SaveParametersToFile();
        }

        // ================================================================
        // ADD / DELETE PARAMETER
        // ================================================================
        private void lvParameters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedRows();
                e.Handled = true;
            }
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            if (_currentFilePath == null)
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewGroup targetGroup = GetSelectedTargetGroup();
            if (targetGroup == null)
            {
                targetGroup = CreateGroup("General");
                RefreshTargetGroupCombo(targetGroup);
            }

            var item = new ListViewItem(new[] { "NewParameter", "0", _parameterTypes[0].FullName });
            item.Group = targetGroup;
            lvParameters.Items.Add(item);

            item.EnsureVisible();
            item.Selected = true;
            item.Focused = true;

            SaveParametersToFile();
            BeginEditSubItem(item, 0);
        }

        private void btnDeleteRow_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void DeleteSelectedRows()
        {
            if (lvParameters.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Please select one or more rows to remove.",
                    "No Row Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove {lvParameters.SelectedItems.Count} parameter(s)?",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            var toRemove = new List<ListViewItem>();
            foreach (ListViewItem item in lvParameters.SelectedItems) toRemove.Add(item);
            foreach (var item in toRemove) lvParameters.Items.Remove(item);

            SaveParametersToFile();
        }

        // ================================================================
        // NEW GROUP
        // ================================================================
        private void btnNewGroup_Click(object sender, EventArgs e)
        {
            if (_currentFilePath == null)
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = PromptForGroupName();
            if (name == null) return; // cancelled

            if (GroupNameExists(name))
            {
                MessageBox.Show($"A group named \"{name}\" already exists.", "Duplicate Group", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var group = CreateGroup(name);
            RefreshTargetGroupCombo(group);

            SaveParametersToFile();
        }

        private bool GroupNameExists(string name)
        {
            foreach (ListViewGroup g in lvParameters.Groups)
                if (string.Equals(g.Header, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static string PromptForGroupName()
        {
            using (var prompt = new Form())
            {
                prompt.Width = 320;
                prompt.Height = 140;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "New Group";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                var label = new Label { Left = 12, Top = 15, Width = 280, Text = "Group name:" };
                var textBox = new TextBox { Left = 12, Top = 38, Width = 280 };
                var okButton = new Button { Text = "OK", Left = 130, Width = 80, Top = 70, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Left = 212, Width = 80, Top = 70, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(label);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(okButton);
                prompt.Controls.Add(cancelButton);
                prompt.AcceptButton = okButton;
                prompt.CancelButton = cancelButton;

                return prompt.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text)
                    ? textBox.Text.Trim()
                    : null;
            }
        }
    }
}
