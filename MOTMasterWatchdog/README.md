# MOTMaster output protection

An NI analog output holds its last value when its task stops. If MOTMaster stops
mid-pattern (Stop pressed, an exception, a crash, End Task), a coil driver can be
left energised. These protections are **opt-in per experiment**; an experiment that
doesn't opt in sees no change.

## Opting in

Declare each analog output to protect, with its safe value in volts, in your
experiment's `Hardware` class (e.g. `DAQ/MoleculeMOTHardware.cs`):

```csharp
Info.Add(DAQ.Analog.DAQMxAnalogSafeState.InfoKey, new Dictionary<string, double>
{
    { "MOTCoilsCurrent", 0.0 },
    { "transferCoils", 0.0 },
});
```

Use names registered with `AddAnalogOutputChannel`. Each value must be inside its
channel's range. To add a channel or change a value, edit this and rebuild. MOTMaster
and the watchdog both read this one list. The watchdog shows it as a read-only table.

Both check the list at startup. If a name isn't registered, or a value is outside its
channel's range, MOTMaster warns you and won't set the safe outputs after runs. The
watchdog shows "NOT PROTECTING" with the reason. Neither writes a partial safe state.

That gives you two layers:

1. **MOTMaster itself** sets those channels to their safe values at the end of every run, however
   the run ends: finished, stopped early, or failed with an exception. If it can't,
   it shows an "outputs NOT made safe" warning.
2. **MOTMasterWatchdog** (this app) runs alongside MOTMaster and covers the cases
   MOTMaster can't cover by itself:
   - **MOTMaster exits** for any reason (crash, End Task, close): the watchdog drives the
     channels to their safe values, retrying for up to 10 s while DAQmx frees the dead process's
     channels. If that fails, the window raises an alarm.
   - **MOTMaster stops responding for 10 s:** the watchdog raises an alarm and offers
     "Kill MOTMaster and make safe". It never kills MOTMaster on its own. Pausing MOTMaster at
     a breakpoint in the debugger triggers this alarm too.
   - **"Set safe outputs now"** is a manual button, enabled only while MOTMaster isn't
     running.

   The watchdog writes nothing when it starts up.

Start the watchdog before MOTMaster and leave it running. It picks the Hardware
class by machine name, the same way MOTMaster does.

## Building

`MOTMasterWatchdog.csproj` builds under any configuration name
(`/p:Configuration=<yours>`). In `EDMSuite.sln` it is only enabled for the CaF solution
configuration. Tick it in Configuration Manager to build it under yours.

## Shared behaviour (all experiments, no opt-in needed)

- Only one run at a time. A second Run click, or a Python `mm.Go(...)` call during a
  run, is ignored and logged. The Run button and "Replicate script" are greyed out
  while a run is in progress.
- An exception during a run no longer kills MOTMaster. The pattern tasks are released,
  a dialog shows the error, and the error is written to AppLog. A Python caller still
  receives the exception.
- Closing the MOTMaster window mid-run ends the run instead of leaving it running in the
  background.
