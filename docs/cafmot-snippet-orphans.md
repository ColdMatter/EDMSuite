# CaF MOTMaster snippets: orphaned lines

Branch `cafmot-snippets`, 2026-10-27. Companion to `docs/cafmot-snippets.md`.

The snippet versions of the scripts (`MoleculeMOTMasterScripts/*_snippets.cs`) still have some lines written out in full, outside any snippet. This note lists the ones that are nearly a snippet, but not quite: the same lines as an existing snippet with one thing changed, such as a parameter name, a typed-in number or which coil is driven. For each group there is a best guess at why it isn't a snippet. Those reasons come from reading the code, not from asking whoever wrote it.

Line numbers are in the `_snippets.cs` files as of commit `f743ad39`. Script names are shortened. `8. load_tweezer`, for example, is `8. OP_transfer_to_tweezer_coils_blueMOT _load_tweezer_snippets.cs`.

## Summary

| Nearly the same as | Lines | Why not a snippet (best guess) |
|---|---|---|
| `Magtrap` | 6 | The field has a different parameter name |
| `LambdaCooling` | 2 pairs | It switches off the tweezer coils, not the MOT coils, and the two scripts name the start time differently |
| `LightOff` | 4 | The DDS frequencies or amplitude parameter differ |
| `TweezerBlueMOT` | 2 pairs | It steps the field instead of ramping it, under different names, or it is a red MOT |
| `HandoverToTransferCoils` / `HandoverToTweezerCoils` | 10 places | Handovers go the other way, use their own time names or have typed-in targets |
| `LoadMOT` | 7 | An old channel name for the trigger, or a channel `LoadMOT` doesn't own |

There are also repeated blocks that have no snippet yet and whose names have drifted apart from script to script. They are covered at the end.

## Like `Magtrap`, but the field has a different name

`Magtrap` sets `MOTCoilsCurrent` at `MagtrapStartTime` to `MOTCoilsMagtrapValue`, which is 1.5 in `globalParameters.json`.

Scripts that use `MagTrapField` instead:

- `7. blueMOT:166`
- `7. blueMOT_temperature:173`
- `7. redMOT:167`
- `8. load_tweezer:178`

Why: these scripts had their own name for the value. Each one sets `MagTrapField` twice in `SetParameters` (1.42, then 1.6), so 1.6 is what runs.

Scripts that use `BlueMOTField` instead:

- `6. OP_back:154`
- `BX_BMOT_OP_magtrap:127`

Why: the magtrap reuses the blue MOT field on purpose and has no value of its own.

Possible fix: set `MOTCoilsMagtrapValue` in these scripts and call `new Magtrap(p, Parameters)`.

## Like `LambdaCooling`, but in the tweezer chamber

`LambdaCooling` adds a DDS event with the lambda frequencies at `<stage>StartTime` and sets `MOTCoilsCurrent` to `MOTCoilsOffValue` at the same moment. These do the same thing in the tweezer chamber:

- `8. load_tweezer:117` (DDS) and `:185` (analog): `TweezerCoils` set to 0 at `tLambdaStartTime`.
- `5. molasses_then_back:85` (DDS) and `:151` (analog): `TweezerCoils` set to 0 at `tMolassesStartTime`.

Why: the snippet always switches off `MOTCoilsCurrent`, not `TweezerCoils`. On top of that, the two scripts give the same stage different names (`tLambdaStartTime` and `tMolassesStartTime`).

## Like `LightOff`, but with different DDS frequencies or amplitude

`LightOff` sets the `ResonanceDDS*` frequencies, with every amplitude set to `LightoffDDS1`.

- `2.a DDSBlueMOT:146` uses amplitude `LightoffDDS2`, at `BlueMOTEndTime`.
- `3. MagTrap_noBM:90` uses the MOT frequencies, at `MagtrapStartTime`.
- `BX_OP_magtrap:68` uses the MOT frequencies, at `OPStartTime`.
- `4. BM_move_coils:173` uses its own set of tweezer frequencies (`SF`, `ResonanceDDS2-4`), at `TweezerLoadEndTime`.

Why: they differ only in which frequency set or amplitude parameter they picked. Every `LightoffDDS*` is 0.0 in `globalParameters.json`, so the frequencies probably make no difference and these are all the same thing in practice.

## Like `TweezerBlueMOT`

- `5. bmot_then_back:86` (DDS) and `:148` (analog) use the same event name and DDS settings as the snippet. Why: they start at `tMagtrapStartTime` and step `TweezerCoils` straight to `tMagTrapField`. The snippet ramps to `tBlueMOTField` over `BlueMOTRampDuration`, starting at `tBlueMOTStartTime`.
- `7. redMOT:109` (DDS) and `:173` (analog) run a red MOT in the tweezer chamber, using the MOT frequencies and amplitudes. Why: there is no red MOT snippet, and the tweezer-coil field is typed in as 1.0.

## Coil handovers that don't match either handover snippet

`HandoverToTransferCoils` ramps the MOT coils to 0 and the transfer coils to 10.0 at `TransferHandoverStartTime`. `HandoverToTweezerCoils` ramps the transfer coils to `TransferCoilsOffValue` and the tweezer coils to `tMagTrapField` at `TweezerHandoverStartTime`.

- `4. TCoil_back_forth:198-199` goes straight from the MOT coils to the tweezer coils at `HandoverStartTime`. Why: it skips the transfer coils, and the tweezer-coil target is typed in as 0.48. Other scripts get the same 0.48 by setting `tMagTrapField = 0.48`.
- `4. Transfer_external_back_to_internal:206-207` hands back from the transfer coils to the MOT coils. Why: it goes the other way. It is also timed off `TweezerHandoverStartTime`, the name `HandoverToTweezerCoils` uses for the opposite move.
- `3. Mag_trap_density_check:206-207` is the same handback as the one above, timed off `HandbackStartTime`.
- Three scripts ramp the transfer coils to 0.0 at `TransferCoilsOffTime`, with nothing else:
  - `4. BlueMOT_transport:192`
  - `4. TCoil_no_movement:193`
  - `4. TCoil_no_movement_BMOT_image:306`

  Why: they use their own time name, and in `HandoverToTweezerCoils` this ramp only comes together with the tweezer-coil ramp.
- The tweezer → transfer → MOT handback block (`TransferHandoverBackStartTime`, then `MOTHandoverBackStartTime`) is identical in four scripts:
  - `5. then_back:235`
  - `5. bmot_then_back:151`
  - `5. molasses_then_back:155`
  - `6. OP_back:161`

  Why: there is no name mismatch here. Nobody has made a snippet for it yet. It is the easiest of all of these to turn into one.

## Like `LoadMOT`

- These three send a t = 0 pulse on `cafOptPumpingAOM`:
  - `1. LambdaRecap:157`
  - `4. TCoil_back_forth:162`
  - `4. TCoil_no_movement:158`

  Why: `cafOptPumpingAOM` is the old name for `DDS_Analog_Trg`, the same line (`digitalPatternBoardAddress2`, port 0, line 2). It was renamed in `b3b41384` (2026-08-03). `LoadMOT` already pulses `DDS_Analog_Trg`, so these lines repeat it. `MoleculeMOTHardware.cs` no longer registers a channel called `cafOptPumpingAOM`, so these scripts will probably fail when building the pattern. The lines can be deleted. The original, non-snippet versions of the same three scripts have the same problem, as does `Spectroscopy_Aug26/QCLSpecinMOT.cs`.
- These four set `transferCoils` to -0.1 at t = 0:
  - `7. blueMOT:159`
  - `7. blueMOT_temperature:166`
  - `7. redMOT:160`
  - `8. load_tweezer:171`

  Why: `LoadMOT` doesn't add or set `transferCoils`. Only these tweezer scripts set its starting value.

## Repeated blocks with no snippet, and names that have drifted

These blocks are not near-copies of an existing snippet, but each one repeats across several scripts with inconsistent names. They would need agreed names before they could become snippets.

- MOT recapture or imaging coils: `MOTCoilsCurrent` is switched on at `ImageTime`, but the switch-off varies:
  - `ImageTime + 2000` in `1. LambdaRecap` and `2.b`
  - `+ Frame0TriggerDuration + 500` in `3. MagTrap_noBM`, `3. MagTrap_withBM` and `BX_OP_magtrap`
  - `+ TweezerImageDur`, going to 0.0 rather than `MOTCoilsOffValue`, in `3. Mag_trap_density_check` and `4. Transfer_external_back_to_internal`

  `2.a` is different again: it switches the coils on with a typed-in 1.0 at `ImageTime - 100`.
- Background frame time is `BackgroundTime` in `3. MagTrap_withBM` and `6. OP_back`, and `BackgroundImageTime` in the `BX_*` scripts.
- MOT shutters:
  - The pulse length is `MagtrapDuration` in some scripts and `ShutterEdgeDur` in others.
  - Timing is measured from `BlueMOTEndTime`, `LambdaCooling1StartTime` or `LambdaCooling2EndTime`, depending on the script.
  - Some are written as `Pulse(0, ...)` with absolute times and others as `Pulse(patternStartBeforeQ, ...)`.
- The YAG compensating shots fire on `YagPreFire` alone in some scripts, and on both `YagPreFire1` and `YagPreFire` in others.
- Tweezer-chamber shutter:
  - `4. BM_move_coils`, `4. TCoil_back_forth` and `4. Transfer_external_back_to_internal` use `test10` (board 2, port 1, line 0).
  - `5. molasses_then_back`, the `7.` scripts and `8.` use `TweezerChamberMOTShutter` (board 1, port 1, line 1).
  - Both carry the comment "Shutter CaF light to tweezer chamber", but they are different physical lines.
