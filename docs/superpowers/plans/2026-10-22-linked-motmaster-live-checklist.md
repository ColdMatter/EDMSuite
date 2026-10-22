# Linked MOTMaster: first live run checklist

This is for the person doing the first live linked run, on PH-BONESAW (primary) and RbTweezer (secondary). Nothing here was run during development: per the repo safety rule, the hardware and remoting paths were checked by building and reading the code only.

## Before you start

1. Close MOTMaster on both PCs. Then build `MOTMaster.csproj` from `feature/linked-motmaster` into the normal `bin` folders: `CaF` on PH-BONESAW and `RbTweezer` on RbTweezer. During development a running MOTMaster held `bin\CaF` locked, so every test build went to a scratch folder instead.
2. Make sure PH-BONESAW can reach `10.59.3.2:1187` (firewall).
3. Wiring: `/PXI1Slot4` port 1 line 6 (`v0ddsSwitchD`) on PH-BONESAW goes to RbTweezer `/PXI1Slot5/PFI5`.

## GUI

1. On PH-BONESAW, a "Linked to 10.59.3.2" checkbox appears next to the Save-batch box. On every other machine, no checkbox appears and the window looks as it did before.
2. Tick Linked while the secondary is running. The checkbox greys out while it connects and the window stays responsive.
3. On RbTweezer, check the lock state:
   - a gold banner reads "Controlled by PH-BONESAW (linked run)"
   - Run, Replicate, Select Script (button and menu), Refresh Script List, the script list and "triggered" are greyed out
   - a Break link button is visible
   - Stop and View Pattern still work
4. Tick Linked while RbTweezer MOTMaster is closed or unreachable. Expect about a 20 s wait (TCP connect timeout) with the window still responsive. Then an error box appears and the checkbox ends up unticked. This wait is expected; it is not a hang.
5. Untick Linked. The banner and Break link button disappear on RbTweezer, and its controls re-enable.
6. During any run on the primary, the Linked checkbox is greyed out and can't be changed.
7. Call `SetLinked(True)` / `SetLinked(False)` from Python on the primary. The checkbox ticks and unticks itself to match.
8. Click Break link on RbTweezer. A confirmation appears, defaulting to No. Yes clears the lock. If a linked run is active, it stops, and the primary's next `Go` raises an error saying to re-link.
9. Visual check: the banner doesn't overlap "triggered" or the Stop button.

## Runs

1. Run a minimal linked script with one RbTweezer digital output and the trigger line on a scope. Measure the trigger-to-output delay and set `SecondaryTriggerDelay` in `globalParameters.json` from it. The value is in clock ticks, at 100 kHz.
2. Check that both run zips contain `_link.txt` with the same `LinkRunId`, the same script and parameter hashes, and both machines' provenance.
3. With the trigger cable pulled, a single-shot `Go` should raise "never saw the trigger". It must not report success.
4. Kill the primary during a linked run. RbTweezer should end its armed run after about one pattern length plus 10 s, showing "no contact from the primary…". The banner stays until you press Break link.
5. With Linked ticked, View Pattern on a CaF-only script shows a message box saying it uses no RbTweezer channels. MOTMaster must not crash.
6. Scripts `0. DDSFourSidebands5W_noDDS` and `BX_spectroscopy` drive `v0ddsSwitchD` themselves:
   - linked: they are refused with "reserved for the secondary trigger"
   - unlinked: they behave exactly as before

## Known limits

- **Clock drift.** Each chassis runs on its own reference clock, so a timing offset builds up between the two systems from each shot's trigger onwards. It resets at every shot. Sharing a 10 MHz reference later would remove it.
- **Pattern viewer.** View Pattern on the primary shows only the primary's channels.
- **DDS prompts.** `armDDS`'s own dialogs, such as the prompt offering to open SpectrumDDSController, can still block a linked Python `Go` until someone clicks. This is the same as for non-linked runs today.
- **Error file paths.** Compiler errors on a linked primary name a temp copy of the script, not your file. Line numbers are unchanged.
