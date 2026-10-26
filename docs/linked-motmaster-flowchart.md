# Linked MOTMaster

One script runs on two machines. PH-BONESAW (primary) runs it as usual and triggers RbTweezer (secondary) with a TTL. Tick **Linked** on PH-BONESAW to turn it on.

## One script, two sets of patterns

```mermaid
flowchart TD
    S["Script"] --> P["PH-BONESAW builds it<br>keeps only its own channels<br>+ adds the rbTrigger pulse"]
    S --> R["RbTweezer builds the same script<br>keeps only its own channels<br>shifts times by SecondaryTriggerDelay"]
```

Each channel name belongs to exactly one machine, which is how each side knows what to keep. Write all times as for a normal cafmot script.

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
