# Linked MOTMaster: how it works

Linked mode lets one MOTMaster script drive the pattern generators of two machines as if they were one experiment. The **primary** (PH-BONESAW, cafmot) runs the script as usual. The **secondary** (RbTweezer) builds its own part of the same script and plays it when the primary sends it a TTL trigger. You still run everything from the primary's window or from Python (`SetScriptPath` / `Go`). Nothing changes for any run with Linked unticked.

The diagrams below are Mermaid, which Obsidian, GitHub and VS Code's Markdown preview all render.

## 1. Linking the two MOTMasters

Linking happens once, before any runs. After that, every `Go` on the primary is a linked run until you untick Linked.

```mermaid
flowchart TD
    A["PH-BONESAW MOTMaster starts<br>MoleculeMOTHardware declares a MOTMasterLinkConfig<br>(secondary URL, trigger channel rbTrigger, analog-start channels)"] --> B["Window shows the 'Linked to ...' checkbox"]
    B --> C["User ticks Linked<br>Controller.SetLinked(true)"]
    C --> D["Builds an RbTweezerHardware object locally<br>(only to know RbTweezer's channel names)"]
    D --> E["ClaimLink over .NET Remoting<br>tcp://10.59.3.2:1187"]
    E --> F{"RbTweezer free?<br>(not running a local pattern)"}
    F -- no --> G["Claim refused:<br>error box on PH-BONESAW, Linked stays off"]
    F -- yes --> H["RbTweezer returns a claim token<br>gold banner: 'controlled by PH-BONESAW'<br>local Run / Go / SetScriptPath refused"]
    H --> I["PH-BONESAW is linked:<br>every Go from now on is a linked run"]
    I --> J{"How does the link end?"}
    J -- "untick Linked on PH-BONESAW" --> K["ReleaseLink: banner goes,<br>RbTweezer usable locally"]
    J -- "'Take back control' on RbTweezer<br>(only between runs)" --> L["Claim broken on RbTweezer.<br>PH-BONESAW finds out on its next Go,<br>which fails and turns Linked off"]
```

## 2. One script becomes patterns for both machines

Both machines compile the **same script text** and call the same `GetSequence()`. The difference is a routing context (`LinkRouting`) that is active while `GetSequence()` runs. Every `Pulse`, `AddEdge`, `AddAnalogValue`, ramp and so on asks it two questions: *is this channel mine?* and *what tick is this on my clock?* Channels belonging to the other machine are silently skipped, so each machine ends up with only its own channels.

```mermaid
flowchart TD
    S["Script file on PH-BONESAW<br>(e.g. LinkedScopeTest.cs)"] --> R["Primary reads the file once<br>and copies it to %TEMP%\MOTMasterLinked"]
    R --> PC["Primary compiles it with its own parameters<br>(globalParameters.json + Go dictionary)"]

    PC --> PG["GetSequence() with primary routing"]
    PG --> PQ{"For each pattern call:<br>whose channel is it?"}
    PQ -- "PH-BONESAW" --> PK["Kept at time t, unchanged"]
    PQ -- "RbTweezer" --> PS["Skipped"]
    PQ -- "rbTrigger" --> PE["Error: reserved for the trigger"]
    PQ -- "both or neither machine" --> PX["Error: every channel name must<br>be on exactly one machine"]
    PK --> PT["MOTMaster adds the rbTrigger pulse at tick 0<br>and notes A = first rise of DDS_Analog_Trg"]
    PT --> PD["Primary patterns built<br>DDS pattern armed as usual (primary only)"]

    PC --> REQ["LinkRunRequest sent to RbTweezer:<br>script text, full parameter set, A, run id, save flag, batch number"]
    REQ --> SC["Secondary compiles the same text<br>LoadGlobalParameters() returns the primary's parameters,<br>not RbTweezer's own json"]
    SC --> SG["GetSequence() with secondary routing"]
    SG --> SQ{"For each pattern call:<br>whose channel is it?"}
    SQ -- "PH-BONESAW" --> SS["Skipped"]
    SQ -- "RbTweezer" --> ST["Time moved to RbTweezer's clock:<br>digital t → t − D<br>analog t → t + A − D<br>(D = SecondaryTriggerDelay)"]
    ST --> SN{"Lands before tick 0?"}
    SN -- "no" --> SK["Kept"]
    SN -- "yes, single edge or value" --> SZ["Moved to tick 0 and logged"]
    SN -- "yes, pulse or ramp" --> SE["Error: refused"]
    SK --> SB["Secondary patterns built<br>(no DDS on the secondary)"]
    SZ --> SB
```

### Times in the script

Write the script in the primary's times, exactly as for an unlinked cafmot script: digital times count from the pattern generator start, analog times from the `DDS_Analog_Trg` pulse. RbTweezer's tick 0 is when the trigger reaches it, `D` ticks after the primary starts. `D` is `SecondaryTriggerDelay` in `globalParameters.json` (General group), and you measure it with `LinkedScopeTest.cs` on a scope.

An example with `D = 3` and `DDS_Analog_Trg` first rising at primary tick `A = 2000`:

| Script says | Plays on PH-BONESAW at | Plays on RbTweezer at |
|---|---|---|
| digital CaF channel at 1000 | tick 1000 | – |
| digital Rb channel at 1000 | – | tick 997 (= 1000 − 3) |
| analog CaF channel at 50 | 50 ticks after `DDS_Analog_Trg` (tick 2050) | – |
| analog Rb channel at 50 | – | tick 2047 (= 50 + 2000 − 3) |

So an Rb event and a CaF event at the same script time happen at the same moment in the lab.

## 3. What happens on Go

This is a single `Go` with several shots. All the arrows between the two MOTMasters are .NET Remoting calls made by the primary. The only exception is the trigger, which is the TTL on `rbTrigger` wired to RbTweezer's `PGTriggerLine`. The secondary never calls the primary; the primary keeps polling it instead.

```mermaid
sequenceDiagram
    actor U as User or Python
    participant P as PH-BONESAW MOTMaster (primary)
    participant S as RbTweezer MOTMaster (secondary)

    U->>P: Go(parameters)
    Note over P: Read script once, compile,<br>build primary patterns + rbTrigger pulse,<br>arm DDS
    P->>S: PrepareLinkedRun(token, LinkRunRequest)
    Note over S: Compile same text with primary's parameters,<br>build RbTweezer patterns,<br>start linked run thread
    S-->>P: LinkReadyInfo (script hash, parameter hash,<br>clock rates, 32/64-bit, git revision)
    Note over P: Refuse the run if bitness, script hash,<br>parameter hash or clocks differ.<br>Different git revision: warning only

    loop each shot
        Note over S: Arm pattern generator,<br>wait for trigger, ArmedShots + 1
        P->>S: GetLinkShotStatus (polled every 5 ms)
        S-->>P: ArmedShots, PlayedShots, Running, Error
        Note over P: Wait until shot n is armed<br>(error after ArmTimeoutMs = 5 s)
        P->>P: Play primary pattern
        P--)S: TTL on rbTrigger at tick 0
        Note over S: Plays its pattern, PlayedShots + 1,<br>releases hardware, re-arms for next shot
    end

    P->>S: GetLinkShotStatus until PlayedShots = shots fired
    Note over P: If not: "never saw the trigger"<br>(check the cable)
    P->>S: EndLinkedRun(token)
    Note over S: Leave loop, save zip with _link.txt,<br>set safe outputs, run ends
    P->>S: GetLinkShotStatus until Running = false
    Note over P: Save zip with _link.txt<br>(same LinkRunId as RbTweezer's)
    P-->>U: Go returns
```

## 4. When something goes wrong

A linked run never fails quietly. Any failure throws out of `Go`: the GUI shows an error box and Python gets an exception. Errors from RbTweezer carry its own message across to PH-BONESAW.

```mermaid
flowchart TD
    F["Failure anywhere in a linked run"] --> W{"Where?"}
    W -- "Before PrepareLinkedRun<br>(compile, routing, DDS arm, Debug mode)" --> X1["Go throws.<br>Nothing was started on RbTweezer"]
    W -- "During PrepareLinkedRun<br>or after it" --> X2["Primary stops its own pattern,<br>sets safe outputs,<br>then tells RbTweezer to end (Abort).<br>Go throws"]
    W -- "RbTweezer reports an error<br>or stops early" --> X2
    W -- "PH-BONESAW dies or goes quiet<br>mid-run" --> X3["RbTweezer gives up after<br>about one pattern + ArmTimeoutMs + 5 s,<br>sets safe outputs and ends its run itself"]
    W -- "RbTweezer user pressed<br>Take back control" --> X4["Next Go on PH-BONESAW fails with<br>'taken back control';<br>Linked turns itself off. Tick it again to re-link"]
```

## What a script needs to run linked

- At least one event on an RbTweezer channel. Otherwise linked mode refuses the run, since the script doesn't need it.
- At least one digital event on RbTweezer's master pattern board, so that RbTweezer's pattern has a clock.
- If there are RbTweezer analog events: a rising edge on `DDS_Analog_Trg` (or `aoPatternTrigger`) in the primary's pattern, which gives `A`.
- A `SecondaryTriggerDelay` parameter (cafmot's `globalParameters.json` already has one).
- No events on `rbTrigger`; MOTMaster drives it.
- No RbTweezer event that would start before RbTweezer is triggered: in practice no pulse or ramp in the first `D` ticks.

## More detail

- `.claude/skills/motmaster-linked-mode/SKILL.md` covers the code layout, files and builds.
- `docs/superpowers/specs/2026-10-22-linked-motmaster-design.md` is the design and the reasons behind it.
