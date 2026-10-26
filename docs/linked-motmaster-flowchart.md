# Linked MOTMaster

One script runs on two machines. PH-BONESAW (primary) runs it as usual and triggers RbTweezer (secondary) with a TTL. Tick **Linked** on PH-BONESAW to turn it on.

## One script, two sets of patterns

```mermaid
flowchart TD
    F["Script file on PH-BONESAW"] --> C1["PH-BONESAW compiles the C#<br>parameters from its globalParameters.json<br>+ any values passed to Go"]
    F --> T["Script text + PH-BONESAW's parameters<br>sent to RbTweezer"]
    T --> C2["RbTweezer compiles the same C#<br>LoadGlobalParameters() returns PH-BONESAW's values,<br>not RbTweezer's own json"]
    C1 --> G1["GetSequence() builds the patterns<br>channel names looked up here"]
    C2 --> G2["GetSequence() builds the patterns<br>channel names looked up here"]
    G1 --> Q1{"Each channel used:<br>in MoleculeMOTHardware<br>or RbTweezerHardware?"}
    G2 --> Q2{"Each channel used:<br>in MoleculeMOTHardware<br>or RbTweezerHardware?"}
    Q1 -- MoleculeMOTHardware --> K1["Keep, time unchanged"]
    Q1 -- RbTweezerHardware --> D1["Drop"]
    Q2 -- RbTweezerHardware --> K2["Keep, time shifted<br>to RbTweezer's clock"]
    Q2 -- MoleculeMOTHardware --> D2["Drop"]
    K1 --> P1["PH-BONESAW patterns<br>+ rbTrigger pulse at t = 0<br>+ DDS pattern"]
    K2 --> P2["RbTweezer patterns"]
```

**Compiling vs building.** These are two separate steps. In the script a channel is just a string, e.g. `p.Pulse(start, 0, duration, "tScopeTest")`, so the C# compiles on either machine whatever channels it uses. Channel names are only looked up when `GetSequence()` runs and builds the patterns. Without linked mode, PH-BONESAW fails at that point on an RbTweezer channel. With linked mode, it skips it.

**What gets compiled.** PH-BONESAW reads the script file once and sends that exact text, with its full parameter set, to RbTweezer. Both machines then compile and run the same `Patterns` class, each against its own build of MOTMaster and `DAQ.dll`. RbTweezer never uses its own `globalParameters.json` or a script file of its own. Before the run starts, each side hashes the script text and the parameters, and PH-BONESAW checks that the two sets of hashes match.

**Which channel belongs to which machine.** Each machine creates both Hardware classes: its own, and the other machine's, created only to read its channel list. Every `Pulse`, `AddEdge`, `AddAnalogValue`, ramp and so on in `GetSequence()` looks its channel name up in both lists. A machine keeps the event if the name is in its own Hardware class and drops it if the name is in the other one. So a channel name must be defined in exactly one of the two Hardware classes: `MoleculeMOTHardware` for CaF channels, `RbTweezerHardware` for Rb ones. The script itself doesn't say which machine a channel is on.

**Times.** Write every time as for a normal cafmot script: digital times from the pattern start, analog times from `DDS_Analog_Trg`. RbTweezer's clock starts when the trigger reaches it, `SecondaryTriggerDelay` ticks after PH-BONESAW's starts. It converts each kept event to its own clock, so an Rb event and a CaF event at the same script time happen together.

**Added automatically.** MOTMaster adds the `rbTrigger` pulse at t = 0, so scripts must not use that channel. The DDS pattern only runs on PH-BONESAW.

## How the two MOTMasters talk

```mermaid
sequenceDiagram
    participant P as PH-BONESAW
    participant R as RbTweezer
    P->>R: script + parameters
    R-->>P: ready (same script and parameters?)
    loop each shot
        P->>R: armed?
        P--)R: TTL trigger
    end
    P->>R: end run
    Note over P,R: both save their data
```

All messages go over the network (.NET Remoting), except the trigger, which goes over a cable.
