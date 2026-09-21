# The two generic MOTMaster-UI features

Both were ported from another experiment's branch (`cafbec`) into `cafmot`, cherry-picked
out of one giant squashed commit that also contained unrelated, since-abandoned DDS
rework — the versions here are the clean, `cafmot`-native ones. Both are generic (no
experiment-specific code) and safe to extend for any experiment on the suite.

## Pattern visualiser — "View Pattern" button

`Controller.ViewPattern()` (in `Controller.cs`, "RUN RUN RUN" region) does everything
`Go()` does up through `buildPattern` — compile the selected script, get its sequence,
build the sample arrays — but stops there instead of touching hardware. It hands the
built arrays to `PatternViewer` (`MOTMaster/PatternViewer.cs`), a self-contained WinForms
`Form` (pure GDI+, no external packages) that plots every digital/analog channel with
scroll + zoom, opened via `ShowDialog(controllerWindow)`.

**This is the one place safe to use for eyeballing a script's timing without running
it.** It calls `buildPattern` but never `initializeHardware`/`run`/`releaseHardware`, so
no NI board is ever configured or triggered.

Wiring: `ControllerWindow`'s `viewPatternButton_Click` → `controller.ViewPattern()`. The
button itself is a renamed dead stub (`preview_button`) that already existed in the
Designer file positioned right where `cafbec` put its own version — reused rather than
adding a second overlapping control.

Channel labelling: `Controller.GenerateListOfDigitalChannelNames(boardAddress)` builds a
`bit → name` map per board from `Environs.Hardware.DigitalOutputChannels`, merging
same-bit duplicate names with `"/"` (see the `rbCoolingAOM`/`broken1` collision in
[architecture.md](architecture.md)) instead of throwing. If you add a channel whose name
collides with an existing one on the same board/bit, the viewer will show both names
joined rather than crashing — but that's a labelling band-aid, not a fix; the underlying
channel map still has two names pointing at one physical bit.

`PatternViewer` internals (`ChannelDescriptor.cs` for the plain data-holder classes,
`PatternViewer.cs` for drawing) are self-contained and don't touch `Controller` or
`Environs` at all — it only knows about the `(name, uint[])` / `(name, double[,])` tuples
and optional `SortedList<int,string>` name maps passed into its constructor. Extending
the viewer (new plot styles, export, etc.) never needs to touch anything outside this
one file.

## Global parameter-file editor — "Parameters → Edit parameter file"

`ParameterWindow` (`MOTMaster/ParameterWindow.cs` + `.Designer.cs` + `.resx`) is a
generic editor for **any** grouped-JSON parameter file in
`Environs.FileSystem.Paths["scriptListPath"]` — not specific to `globalParameters.json`.
On load it lists every `*.json` in that folder in a combo box; picking one loads its
parameters into a grouped `ListView` (`lvParameters`, `View.Details`, `ShowGroups`).
Every change — an inline cell edit (double-click a cell; text box, or a type dropdown for
the Type column), an added row (`btnAddRow_Click`, into the group picked in
`cmbTargetGroup`), a deleted row (`btnDeleteRow_Click` / Delete key / right-click
Delete), a drag between groups, a right-click **Move to Group**, or a new group
(`btnNewGroup_Click`) — immediately autosaves the whole file via
`ParameterFileManager.WriteFile`. Group headers are click-collapsible; the collapse
plumbing (`GroupHeaderClickWindow`, `LVM_SETGROUPINFO`) is a big chunk of the file and is
purely cosmetic — it does not affect what gets saved or loaded.

On-disk shape: `{ "groups": [ { "name": ..., "parameters": [ { "name", "value", "type" }, ... ] } ] }`.

`ParameterEntry` (`ParameterEntry.cs`) is a plain `{Name, Value, Type}` holder and
`ParameterGroup` (`ParameterGroup.cs`) is a `{Name, List<ParameterEntry>}` holder —
`Value` is always stored/round-tripped as a `string`; the `Type` (`System.Int32` /
`System.Double`, `_parameterTypes` in `ParameterWindow.cs`) only matters when something
later calls `Convert.ChangeType(entry.Value, entry.Type, ...)` — i.e.
`MOTMasterScript.LoadGlobalParameters()`. The editor never validates that `Value` parses
as the selected `Type`; a bad edit (e.g. `abc` in a `System.Double` row) saves fine and
only fails later, as an unhandled exception, the next time a script loads it.

`ParameterFileManager` (`ParameterFileManager.cs`) is the shared I/O layer, using
`System.Web.Script.Serialization.JavaScriptSerializer`. `ReadFile` returns
`List<ParameterGroup>` — tolerates a missing file (empty list) and an unrecognised `Type`
string (falls back to the caller-supplied `fallbackType`); `WriteFile` always writes
`Type.FullName`. `Flatten(groups)` collapses the groups to a flat `List<ParameterEntry>`
for callers that don't care about grouping (`LoadGlobalParameters`), and **throws** if
one parameter name appears in more than one group. Grouping is a pure UI/organisational
convenience — scripts see a flat namespace. Any `*.json` editable through this GUI could
be loaded by a script the same way; the name `globalParameters.json` is just the
convention `LoadGlobalParameters()` looks for.

**Menu wiring:** `ControllerWindow.Designer.cs` adds a top-level `Parameters` menu with
one child `Edit parameter file`; `ControllerWindow.cs`'s
`editParameterFileToolStripMenuItem_Click` just does `new ParameterWindow().Show()` — a
non-modal window, so it can stay open alongside the main controller while you flip
between scripts.

**Extending it:** if you need a second window (e.g. a filtered view, or read-only mode),
`ParameterFileManager` is already the reusable piece — build on that rather than copying
`ParameterWindow`'s grid-wiring code.
