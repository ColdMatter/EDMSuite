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
generic editor for **any** `Name\tValue\tType` text file in
`Environs.FileSystem.Paths["scriptListPath"]` — not specific to `globalParameters.txt`.
On load it lists every `*.txt` in that folder in a combo box; picking one loads its rows
into an editable `DataGridView`; every cell edit, added row, or deleted row immediately
autosaves back to disk via `ParameterFileManager.WriteFile` (`dgvParameters_CellEndEdit`,
`btnAddRow_Click`, `dgvParameters_UserDeletedRow` / `btnDeleteRow_Click`).

`ParameterEntry` (`ParameterEntry.cs`) is a plain `{Name, Value, Type}` holder — `Value`
is always stored/round-tripped as a `string`; the `Type` (`System.Int32` /
`System.Double`, chosen from the `colType` dropdown, `_parameterTypes` in
`ParameterWindow.cs`) only matters when something later calls
`Convert.ChangeType(entry.Value, entry.Type, ...)` — i.e. `MOTMasterScript.LoadGlobalParameters()`.
The editor itself never validates that `Value` actually parses as the selected `Type`; a
bad edit (e.g. typing `abc` into a row typed `System.Double`) saves fine and only fails
later, as an unhandled `FormatException`, the next time a script tries to load it.

`ParameterFileManager` (`ParameterFileManager.cs`) is the shared I/O layer — `ReadFile`
tolerates a missing file (returns empty list) and an unrecognised `Type` string (falls
back to the caller-supplied `fallbackType`); `WriteFile` always writes `Type.FullName`.
This is the same class `MOTMasterScript.LoadGlobalParameters()` uses to read
`globalParameters.txt`, so any file editable through this GUI could in principle be
loaded by a script the same way — the naming (`globalParameters.txt`) is just a
convention, not something the format enforces.

**Menu wiring:** `ControllerWindow.Designer.cs` adds a top-level `Parameters` menu with
one child `Edit parameter file`; `ControllerWindow.cs`'s
`editParameterFileToolStripMenuItem_Click` just does `new ParameterWindow().Show()` — a
non-modal window, so it can stay open alongside the main controller while you flip
between scripts.

**Extending it:** if you need a second window (e.g. a filtered view, or read-only mode),
`ParameterFileManager` is already the reusable piece — build on that rather than copying
`ParameterWindow`'s grid-wiring code.
