# Testing what's causing the debug-log pauses

## What this is for

At debug log level 3 ("log all"), arming the DDS card for a shot has been seen to
pause for tens of seconds at a time, and this can freeze the SpectrumDDSController
window too. The most likely causes are:

1. **Antivirus** re-scanning the log file every time something is written to it.
2. **A backup "snapshot" (VSS)** of the whole disk drive briefly freezing all
   writes to it, not just this program's.

This guide tests each one, using only tools already on a Windows PC (plus one
small free download for the last, optional part). Do these steps one at a time,
in order — Part A first, then B, then C only if A and B don't give a clear
answer.

You do **not** need to run a real experiment for any of this. Use the
**Test trigger** button on the Pattern tab in SpectrumDDSController instead —
it fires the DDS card in software only and touches nothing else, so it's safe
to press repeatedly at any time.

---

## Step 0: Find the log file's folder

1. Open SpectrumDDSController if it isn't already running, and click the
   **Status** tab.

   *This is the DDS control window. The Status tab shows where the debug log
   is currently being written.*

2. Look at the **Debug log level / path** row and write down the folder path
   shown there.

   *You'll need this exact folder in the steps below. Don't assume it's the
   same as last time — check it.*

---

## Part A — Test whether antivirus is the cause

This is the quickest test. You temporarily tell the antivirus to skip the log
folder, then check whether the pauses stop.

1. Press the **Windows key**, type `Windows Security`, and press **Enter**.

   *This opens Windows' built-in antivirus control panel. If the computer uses
   a different antivirus program instead (check for its icon near the clock,
   bottom-right of the screen), open that program's settings instead and look
   for something called "exclusions" or "exceptions" — the idea is the same.*

2. Click **Virus & threat protection**, then click **Manage settings** under
   "Virus & threat protection settings".

   *This is where you tell the antivirus to leave specific folders alone.*

3. Scroll down to **Exclusions**, click **Add or remove exclusions**, click
   **+ Add an exclusion**, choose **Folder**, and select the log folder from
   Step 0.

   *This stops the antivirus scanning every file in that folder, including the
   log file, which grows with every single register write at log level 3. If
   the antivirus was the cause, the pauses should now stop.*

   *If this option is greyed out, the antivirus is controlled centrally by IT
   and you'll need to ask them to add the exclusion for you.*

4. In SpectrumDDSController, on the Status tab, set the debug log level to
   **3**, then click **Close card** and **Open card** again.

   *A level change only takes effect on a fresh connection — closing and
   reopening the card is what makes the new level active. Level 3 is the
   setting that produces the pauses.*

5. Go to the Pattern tab and press **Test trigger** repeatedly (or leave it
   running) for about as long as it previously took to see a pause, while
   watching whether the window stays responsive.

   *If the pauses don't happen anymore with the exclusion in place, antivirus
   scanning was very likely the cause. If a pause still happens, move on to
   Part B.*

6. Afterwards, set the debug log level back down to **0** and reopen the card.

   *Level 3 is expensive to leave running all the time, separately from this
   test — turn it back down once you're done.*

---

## Part B — Test whether a backup snapshot (VSS) is the cause

Windows can take a "snapshot" of an entire disk drive for backup purposes.
While it does, every program's writes to that drive can briefly freeze — not
just this one.

1. Press the **Windows key**, type `PowerShell`, right-click
   **Windows PowerShell**, choose **Run as administrator**, and click **Yes**
   if asked for permission.

   *PowerShell is Windows' built-in command tool. Administrator rights are
   needed to ask Windows about snapshots.*

2. Type the following exactly and press **Enter**:

   ```
   vssadmin list shadows
   ```

   *This lists every snapshot Windows currently has, with the exact date and
   time each was created. Compare these times against any pause you've
   already noted — a close match is a strong sign.*

3. Type the following and press **Enter**:

   ```
   vssadmin list shadowstorage
   ```

   *This shows which drives are set up to take snapshots at all, confirming
   whether the drive holding the log file is one of them.*

4. Press the **Windows key**, type `Event Viewer`, and press **Enter**.

   *This is Windows' built-in record of system events, including every time a
   snapshot is created.*

5. On the left-hand tree, expand **Windows Logs** and click **Application**.

   *Snapshot activity is usually recorded here. If you don't find anything in
   the next step, also check **System** in the same tree.*

6. Press **Ctrl+F**, type `VSS`, and click **Find Next**.

   *Searches for entries from the Volume Shadow Copy service — the Windows
   component responsible for taking snapshots.*

7. For each result, note its date and time, and compare against your pause
   timestamps.

   *A snapshot logged at the same moment as a pause points straight at the
   cause.*

8. Press the **Windows key**, type `Task Scheduler`, and press **Enter**.

   *Shows anything Windows runs automatically on a timer, including scheduled
   backups.*

9. In the task list, look for anything with "backup" or "shadow copy" in its
   name, and check its **Triggers** tab to see when it's scheduled to run.

   *If a backup job's schedule lines up with your pauses, that's the cause —
   and you can ask IT to reschedule it, or avoid testing during that window.*

---

## Part C (optional) — Catch the exact stalled write directly

Only do this if Parts A and B didn't give a clear answer. It uses a small,
free, official Microsoft tool called **Process Monitor**, which isn't
installed by default and needs downloading first.

1. Open a web browser and search for `Microsoft Sysinternals Process Monitor
   download`. Open the official Microsoft page (the web address starts with
   `learn.microsoft.com`) and download it.

   *Process Monitor records every file operation on the computer as it
   happens, with precise timing. It's the only way to see the individual
   write to the log file and how long it took.*

2. Unzip the downloaded file, double-click **Procmon.exe** inside, and click
   **Yes** / **Agree** on the prompts that appear.

   *Launches the tool. It starts recording immediately — that's normal, you
   don't need to press anything else yet.*

3. Click **Filter** in the menu bar, then **Filter...**.

   *Without a filter, Process Monitor records far too much to find anything
   useful in.*

4. Set the first dropdown to **Path**, the second to **contains**, type
   `spcmdrv_debug.txt` in the box, make sure **Include** is selected, click
   **Add**, then **OK**.

   *Limits the display to only entries that touch the DDS debug log file.*

5. Right-click any column header in the main list, choose
   **Select Columns...**, tick **Duration**, and click **OK**.

   *Adds a column showing exactly how long each operation took — this is what
   will reveal the slow write.*

6. With the debug log level still at 3, go back to SpectrumDDSController and
   press **Test trigger** repeatedly until a pause happens, with Process
   Monitor left running in the background.

   *You need a pause to happen while Process Monitor is capturing, so it
   catches the exact moment.*

7. Once you've seen a pause, press **Ctrl+E** in Process Monitor (or click
   **Capture** in the toolbar) to stop recording.

   *Freezes the list so it stops growing while you look through it.*

8. Click the **Duration** column header to sort the list with the longest
   operation at the top.

   *That top row is your stalled write.*

9. Click that row, then remove the filter from step 4 (**Filter → Filter...**,
   select the row you added, click **Remove**, **OK**), and look at everything
   else happening in the few seconds before and after that timestamp —
   especially rows from a process you don't recognise, or one containing
   "Defender", "MsMpEng", your antivirus program's name, or "VSS" / "vssvc".

   *This shows what else the computer was doing at the exact moment of the
   stall, pointing directly at antivirus, a snapshot, or something else
   entirely as the real cause.*

---

## When you're done

Set the debug log level back to a low value (e.g. **0**) and reopen the card,
unless you're actively continuing the investigation. Running at level 3 all
the time is expensive on its own, separately from whatever caused these
particular pauses.
