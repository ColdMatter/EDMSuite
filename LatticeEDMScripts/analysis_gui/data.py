"""Finding ScanMaster files in a folder and loading them in the background.

Files are picked as in the batch *_SL.py scripts: a keyword pattern in one
folder (usually a date) and an optional list of file IDs (the "sele" list,
the number before the first "_"); no IDs means every matching file.
Loaded scans are kept in a cache so re-running an analysis, or loading the
same file in another tab, doesn't read the zip again."""

import glob
import os
import re
import threading
import traceback

from qtpy import QtCore

import LatticeEDM_analysis_library as EDM


def file_id(path):
    """The file ID, e.g. "004" for 004_4fv1_R_setpointScan.zip"""
    return re.split(r'[_]', os.path.basename(path))[0]


def label_id(label):
    """The file ID of a scan label, e.g. "004" for "004_1" (pass 1 of 004)"""
    return label.split("_")[0].split("#")[0]


def parse_ids(text):
    """File IDs from a comma/space separated string, in order, no repeats"""
    ids = []
    for part in re.split(r"[,\s;]+", text.strip()):
        part = part.strip().strip("'\"")
        if part and part not in ids:
            ids.append(part)
    return ids


def keyword_pattern(keyword):
    """Glob pattern for a keyword: "Duration" -> "*Duration*.zip". A keyword
    with wildcards is used as it is (.zip added if missing)."""
    keyword = keyword.strip()
    if not any(c in keyword for c in "*?["):
        keyword = "*" + keyword + "*"
    if not keyword.lower().endswith(".zip"):
        keyword += ".zip"
    return keyword


def find_files(folder, keyword):
    """[(file ID, path)] of the matching files in folder, sorted by name"""
    if not folder or not os.path.isdir(folder):
        return []
    paths = glob.glob(os.path.join(glob.escape(folder), keyword_pattern(keyword)))
    return [(file_id(p), p) for p in sorted(paths, key=lambda p: os.path.basename(p))]


class ScanCache:
    """Scans read from zip files, kept until cleared. Keyed by path, file
    modification time and size, and whether passes or the average were read,
    so a file that changed on disk is read again."""

    def __init__(self):
        self._scans = {}
        # .NET deserialisation is done one file at a time, whichever tab asks
        self.read_lock = threading.Lock()

    @staticmethod
    def key(path, passes):
        st = os.stat(path)
        return (os.path.normcase(os.path.abspath(path)), st.st_mtime, st.st_size, passes)

    def get(self, path, passes):
        return self._scans.get(self.key(path, passes))

    def put(self, path, passes, arrays_list):
        self._scans[self.key(path, passes)] = arrays_list

    def clear(self):
        self._scans.clear()

    def __len__(self):
        return len(self._scans)


CACHE = ScanCache()


def read_file(path, passes):
    """List of scan arrays (EDM.ExtractScanArrays) of one zip file: one per
    pass, or just the average scan"""
    with CACHE.read_lock:
        cached = CACHE.get(path, passes)
        if cached is not None:
            return cached, True
        if passes:
            scans = EDM.ReadAllScansInZippedXML(path)
            if len(scans) == 0:
                raise ValueError("no scans could be read from " + os.path.basename(path))
        else:
            scans = [EDM.ReadAverageScanInZippedXML(path)]
        arrays_list = []
        for scan in scans:
            arrays = EDM.ExtractScanArrays(scan)
            arrays["file"] = path
            arrays_list.append(arrays)
        CACHE.put(path, passes, arrays_list)
        return arrays_list, False


def scan_labels(file_ids, counts, passes):
    """Labels of the scans of each file, as the scripts: "<ID>_<pass>" with
    passes, "<ID>" for the average. Two files with the same ID (it happens)
    carry on the pass numbering, or get "#2" for averages."""
    labels = []
    next_pass = {}
    for fid, n in zip(file_ids, counts):
        if passes:
            start = next_pass.get(fid, 0)
            labels.append([fid + "_%d" % k for k in range(start, start + n)])
            next_pass[fid] = start + n
        else:
            seen = next_pass.get(fid, 0)
            labels.append([fid if seen == 0 else fid + "#%d" % (seen + 1)])
            next_pass[fid] = seen + 1
    return labels


class LoadWorker(QtCore.QThread):
    """Reads a list of (file ID, path) in the background. Emits loaded with
    {label: arrays} (in file order) and a list of problems."""
    progress = QtCore.Signal(int, int, str)
    loaded = QtCore.Signal(object, object)

    def __init__(self, files, passes, parent=None):
        super().__init__(parent)
        self.files = files
        self.passes = passes
        self._cancel = False

    def cancel(self):
        self._cancel = True

    def run(self):
        results = []
        problems = []
        for i, (fid, path) in enumerate(self.files):
            if self._cancel:
                problems.append("Loading cancelled")
                break
            name = os.path.basename(path)
            self.progress.emit(i, len(self.files), name)
            try:
                arrays_list, cached = read_file(path, self.passes)
                print("%s %s (%d scan%s)" % ("Cached" if cached else "Loaded", name,
                                            len(arrays_list), "" if len(arrays_list) == 1 else "s"))
                results.append((fid, arrays_list))
            except Exception as e:
                print(traceback.format_exc())
                problems.append("%s: %s" % (name, e))
        self.progress.emit(len(self.files), len(self.files), "")

        labels = scan_labels([fid for fid, _ in results],
                             [len(a) for _, a in results], self.passes)
        scans = {}
        for (fid, arrays_list), file_labels in zip(results, labels):
            for label, arrays in zip(file_labels, arrays_list):
                scans[label] = arrays
        dup = sorted({fid for fid, _ in results if [f for f, _ in results].count(fid) > 1})
        if dup:
            problems.append("More than one file with ID " + ", ".join(dup) +
                            " - their scans are numbered on from each other")
        self.loaded.emit(scans, problems)
