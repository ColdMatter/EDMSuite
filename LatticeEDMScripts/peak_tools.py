# -*- coding: utf-8 -*-
import tkinter as tk
from tkinter import filedialog, messagebox, ttk

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from matplotlib.backends.backend_tkagg import (
    FigureCanvasTkAgg,
    NavigationToolbar2Tk,
)
from matplotlib.lines import Line2D


class PeakAssignerGUI:
    """Interactive GUI for assigning and labeling YbF spectral peaks.

    Features:
      - Plots measured spectrum as frequency offset (GHz) from center_freq_thz.
      - Overlays vertical prediction stick labels with hyperfine state formatting.
      - Dynamic Zoom-in / Zoom-out and adjustable window range.
      - User-defined X-axis scroll bounds (X-Min and X-Max) directly on the GUI.
      - Supports multiple label assignments per peak (secondary labels omit arrows).
      - Stores assignments in-memory and handles CSV import/export.
    """

    def __init__(
        self,
        root,
        frequencies,
        intensities,
        peak_indices,
        peak_positions,
        center_freq_thz=568.681,
        frequencies_in_thz=True,
        dfs_by_isotope=None,
        stick_offset_ghz=-3.15,
        x_branches_hyperfine=None,
        zoom_window_ghz=1.5,
        x_min_ghz=None,
        x_max_ghz=None,
    ):
        self.root = root
        self.root.title("YbF Peak Assignment & Labeling Tool")
        self.root.geometry("1280x940")

        self.center_freq_thz = float(center_freq_thz)
        self.frequencies_in_thz = frequencies_in_thz
        self.dfs_by_isotope = dfs_by_isotope

        # Hyperfine branch pattern mapping
        if x_branches_hyperfine is None:
            self.x_branches_hyperfine = {
                "O": ["N+", "N-1"],
                "P": ["N+1", "N-", "N+", "N-1"],
                "Q": ["N+1", "N-", "N+", "N-1"],
                "R": ["N+1", "N-"],
            }
        else:
            self.x_branches_hyperfine = x_branches_hyperfine

        # Parse stick_offset_ghz: accepts float, int, or dict
        self.stick_offset_map = {}
        if isinstance(stick_offset_ghz, dict):
            for k, v in stick_offset_ghz.items():
                digits = "".join(filter(str.isdigit, str(k)))
                if digits:
                    self.stick_offset_map[int(digits)] = float(v)
        elif isinstance(stick_offset_ghz, (int, float)):
            default_val = float(stick_offset_ghz)
            self.stick_offset_map = {
                174: default_val,
                172: default_val,
                176: default_val,
                173: default_val,
                171: default_val,
            }

        # Standardize experimental frequencies to GHz Offset relative to center_freq_thz
        raw_freqs = np.asarray(frequencies, dtype=float)
        if self.frequencies_in_thz:
            self.freqs_ghz = (raw_freqs - self.center_freq_thz) * 1000.0
        else:
            self.freqs_ghz = raw_freqs

        self.intensities = np.asarray(intensities, dtype=float)
        self.peak_indices = np.asarray(peak_indices, dtype=int)

        # Standardize peak positions to GHz Offset
        raw_peaks = np.asarray(peak_positions, dtype=float)
        if self.frequencies_in_thz:
            self.peak_positions_ghz = (
                raw_peaks - self.center_freq_thz
            ) * 1000.0
        else:
            self.peak_positions_ghz = raw_peaks

        self.num_peaks = len(self.peak_indices)
        self.current_idx = 0
        self.zoom_window = float(zoom_window_ghz)

        # Set initial scrollable bounds (X-Min and X-Max)
        default_min_f = (
            float(np.min(self.freqs_ghz)) if len(self.freqs_ghz) > 0 else -100.0
        )
        default_max_f = (
            float(np.max(self.freqs_ghz)) if len(self.freqs_ghz) > 0 else 100.0
        )

        self.scroll_xmin = (
            float(x_min_ghz) if x_min_ghz is not None else default_min_f
        )
        self.scroll_xmax = (
            float(x_max_ghz) if x_max_ghz is not None else default_max_f
        )

        # Dynamic label spacing parameters
        self.label_spacing_pt = 18.0  # Vertical step in points per tier/label
        self.collision_radius_ghz = (
            0.35  # Horizontal threshold (GHz) to stagger labels
        )

        # Calculate constant global Y-limits across entire spectrum (1.35x headroom for labels)
        valid_y = self.intensities[np.isfinite(self.intensities)]
        if len(valid_y) > 0:
            self.y_min_global = min(-0.02, float(np.min(valid_y)))
            self.y_max_global = float(np.max(valid_y)) * 1.35
        else:
            self.y_min_global = -0.02
            self.y_max_global = 1.0

        # Data structure: self.assignments[peak_idx] = [label_dict_1, label_dict_2, ...]
        self.assignments = {}
        self._updating_slider = False

        self._prepare_predictions()
        self._build_ui()
        self._plot_spectrum()
        self._load_current_peak_data()

    def _prepare_predictions(self):
        """Precomputes predicted transition frequencies into GHz offsets relative to center frequency with isotope line styles."""
        self.prediction_sticks = []
        if not self.dfs_by_isotope:
            return

        for iso, df_iso in self.dfs_by_isotope.items():
            iso_digits = "".join(filter(str.isdigit, str(iso)))
            iso_int = int(iso_digits) if iso_digits else 174
            iso_offset = self.stick_offset_map.get(iso_int, 0.0)

            # Assign line style by isotope
            if iso_int == 174:
                ls = "solid"
            elif iso_int == 172:
                ls = "dashed"
            elif iso_int == 176:
                ls = "dotted"
            else:
                ls = "dashdot"

            branch_col = next(
                (c for c in ["Branch", "branch"] if c in df_iso.columns), None
            )
            q_col = next(
                (
                    c
                    for c in ["N''", "N_double_prime", 'N"', "N"]
                    if c in df_iso.columns
                ),
                None,
            )
            freq_col = next(
                (
                    c
                    for c in ["Frequency_THz", "Frequency", "Frequency (THz)"]
                    if c in df_iso.columns
                ),
                None,
            )
            pop_col = next(
                (
                    c
                    for c in ["Population", "Intensity", "pop"]
                    if c in df_iso.columns
                ),
                None,
            )

            if not (branch_col and q_col and freq_col):
                continue

            for _, row in df_iso.iterrows():
                branch_name = str(row[branch_col])
                q_val = row[q_col]
                pop_val = (
                    float(row[pop_col])
                    if (pop_col and not pd.isna(row[pop_col]))
                    else 1.0
                )

                raw_val = row[freq_col]
                freq_list = (
                    raw_val
                    if isinstance(raw_val, (list, tuple, np.ndarray))
                    else [raw_val]
                )
                hf_patterns = self.x_branches_hyperfine.get(branch_name, [])

                n_str = (
                    str(int(q_val))
                    if float(q_val).is_integer()
                    else f"{q_val:.1f}"
                )

                for hf_idx, raw_freq in enumerate(freq_list):
                    if raw_freq is None or pd.isna(raw_freq):
                        continue
                    pred_offset_ghz = (
                        (float(raw_freq) - self.center_freq_thz) * 1000.0
                    ) + iso_offset

                    if hf_patterns:
                        hf_str = hf_patterns[hf_idx % len(hf_patterns)]
                        stick_label = f"{n_str}, {hf_str}"
                    else:
                        stick_label = n_str

                    self.prediction_sticks.append({
                        "isotope": iso_int,
                        "branch": branch_name,
                        "q_num": q_val,
                        "hf_comp": hf_idx + 1,
                        "x_offset": pred_offset_ghz,
                        "pop": pop_val,
                        "linestyle": ls,
                        "label": stick_label,
                    })

    def _build_ui(self):
        self.plot_frame = ttk.Frame(self.root)
        self.plot_frame.pack(
            side=tk.TOP, fill=tk.BOTH, expand=True, padx=5, pady=5
        )

        # Plot Canvas & Navigation Toolbar
        self.fig, self.ax = plt.subplots(figsize=(10, 4.5), dpi=100)
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.plot_frame)
        self.canvas.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)

        toolbar = NavigationToolbar2Tk(self.canvas, self.plot_frame)
        toolbar.update()

        # Rolling X-Axis Slider Frame WITH BOUNDS CONTROLS
        slider_frame = ttk.Frame(self.plot_frame)
        slider_frame.pack(side=tk.BOTTOM, fill=tk.X, padx=10, pady=2)

        ttk.Label(
            slider_frame, text="Scroll X-Axis (GHz):", font=("Arial", 9, "bold")
        ).pack(side=tk.LEFT, padx=(5, 2))

        # X-Min Entry
        ttk.Label(slider_frame, text="Min:").pack(side=tk.LEFT, padx=(5, 2))
        self.ent_xmin = ttk.Entry(slider_frame, width=8)
        self.ent_xmin.insert(0, f"{self.scroll_xmin:.2f}")
        self.ent_xmin.pack(side=tk.LEFT, padx=2)

        # Slider
        self.slider_x = ttk.Scale(
            slider_frame,
            from_=self.scroll_xmin,
            to=self.scroll_xmax,
            orient=tk.HORIZONTAL,
            command=self._on_slider_scroll,
        )
        self.slider_x.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=5)

        # X-Max Entry
        ttk.Label(slider_frame, text="Max:").pack(side=tk.LEFT, padx=(2, 2))
        self.ent_xmax = ttk.Entry(slider_frame, width=8)
        self.ent_xmax.insert(0, f"{self.scroll_xmax:.2f}")
        self.ent_xmax.pack(side=tk.LEFT, padx=2)

        # Set Scroll Range Button
        btn_set_scroll = ttk.Button(
            slider_frame,
            text="Set Scroll Range",
            command=self._update_x_scroll_bounds,
        )
        btn_set_scroll.pack(side=tk.LEFT, padx=(5, 5))

        # Control Panel Frame
        self.control_frame = ttk.LabelFrame(
            self.root, text=" Peak Assignment, Layout & View Controls "
        )
        self.control_frame.pack(side=tk.BOTTOM, fill=tk.X, padx=10, pady=5)

        # Row 1: Navigation & Zoom Controls
        nav_frame = ttk.Frame(self.control_frame)
        nav_frame.pack(fill=tk.X, padx=5, pady=3)

        self.btn_prev = ttk.Button(
            nav_frame, text="◄ Previous Peak", command=self.prev_peak
        )
        self.btn_prev.pack(side=tk.LEFT, padx=3)

        self.btn_next = ttk.Button(
            nav_frame, text="Next Peak ►", command=self.next_peak
        )
        self.btn_next.pack(side=tk.LEFT, padx=3)

        self.lbl_peak_info = ttk.Label(
            nav_frame, text="", font=("Arial", 10, "bold")
        )
        self.lbl_peak_info.pack(side=tk.LEFT, padx=15)

        # Zoom controls
        ttk.Label(nav_frame, text="Zoom Window (GHz):").pack(
            side=tk.RIGHT, padx=(10, 2)
        )
        self.spin_window = ttk.Spinbox(
            nav_frame,
            from_=0.1,
            to=100.0,
            increment=0.5,
            width=6,
            command=self._update_zoom,
        )
        self.spin_window.set(self.zoom_window)
        self.spin_window.pack(side=tk.RIGHT, padx=2)

        btn_zoom_in = ttk.Button(
            nav_frame, text="🔍 Zoom In", width=9, command=self.zoom_in
        )
        btn_zoom_in.pack(side=tk.RIGHT, padx=2)

        btn_zoom_out = ttk.Button(
            nav_frame, text="🔎 Zoom Out", width=10, command=self.zoom_out
        )
        btn_zoom_out.pack(side=tk.RIGHT, padx=2)

        # Row 2: Range, Y-Max, and Dynamic Label Spacing Controls
        range_frame = ttk.Frame(self.control_frame)
        range_frame.pack(fill=tk.X, padx=5, pady=3)

        ttk.Label(
            range_frame, text="Center Freq (THz):", font=("Arial", 9, "bold")
        ).pack(side=tk.LEFT, padx=4)
        self.ent_center_freq = ttk.Entry(range_frame, width=10)
        self.ent_center_freq.insert(0, str(self.center_freq_thz))
        self.ent_center_freq.pack(side=tk.LEFT, padx=2)

        btn_recenter = ttk.Button(
            range_frame, text="Apply Freq", command=self.recenter_frequency
        )
        btn_recenter.pack(side=tk.LEFT, padx=(2, 10))

        ttk.Label(range_frame, text="Y-Max:").pack(side=tk.LEFT, padx=(5, 2))
        self.ent_ymax = ttk.Entry(range_frame, width=7)
        self.ent_ymax.insert(0, f"{self.y_max_global:.3f}")
        self.ent_ymax.pack(side=tk.LEFT, padx=2)

        btn_apply_ymax = ttk.Button(
            range_frame, text="Set Y-Max", command=self._apply_manual_ymax
        )
        btn_apply_ymax.pack(side=tk.LEFT, padx=(2, 15))

        # Dynamic Label Layout Controls
        ttk.Label(
            range_frame, text="Label Gap (pt):", font=("Arial", 9, "bold")
        ).pack(side=tk.LEFT, padx=(5, 2))
        self.spin_v_spacing = ttk.Spinbox(
            range_frame,
            from_=5,
            to=60,
            increment=2,
            width=5,
            command=self._update_label_layout,
        )
        self.spin_v_spacing.set(int(self.label_spacing_pt))
        self.spin_v_spacing.pack(side=tk.LEFT, padx=2)

        ttk.Label(
            range_frame,
            text="Overlap Radius (GHz):",
            font=("Arial", 9, "bold"),
        ).pack(side=tk.LEFT, padx=(10, 2))
        self.spin_collision = ttk.Spinbox(
            range_frame,
            from_=0.05,
            to=2.0,
            increment=0.05,
            width=6,
            command=self._update_label_layout,
        )
        self.spin_collision.set(f"{self.collision_radius_ghz:.2f}")
        self.spin_collision.pack(side=tk.LEFT, padx=2)

        btn_apply_layout = ttk.Button(
            range_frame, text="Update Layout", command=self._update_label_layout
        )
        btn_apply_layout.pack(side=tk.LEFT, padx=5)

        # Row 3: Label Assignment Entry Fields
        input_frame = ttk.Frame(self.control_frame)
        input_frame.pack(fill=tk.X, padx=5, pady=5)

        ttk.Label(input_frame, text="Isotope:").grid(
            row=0, column=0, padx=4, pady=2, sticky=tk.E
        )
        self.combo_isotope = ttk.Combobox(
            input_frame,
            values=["174YbF", "172YbF", "176YbF", "Unassigned"],
            width=12,
        )
        self.combo_isotope.set("174YbF")
        self.combo_isotope.grid(row=0, column=1, padx=4, pady=2)

        ttk.Label(input_frame, text="Branch:").grid(
            row=0, column=2, padx=4, pady=2, sticky=tk.E
        )
        self.combo_branch = ttk.Combobox(
            input_frame, values=["O", "P", "Q", "R"], width=8
        )
        self.combo_branch.set("O")
        self.combo_branch.grid(row=0, column=3, padx=4, pady=2)

        ttk.Label(input_frame, text="N:").grid(
            row=0, column=4, padx=4, pady=2, sticky=tk.E
        )
        self.spin_n = ttk.Spinbox(input_frame, from_=0, to=50, width=6)
        self.spin_n.set(1)
        self.spin_n.grid(row=0, column=5, padx=4, pady=2)

        ttk.Label(input_frame, text="Component:").grid(
            row=0, column=6, padx=4, pady=2, sticky=tk.E
        )
        self.combo_component = ttk.Combobox(
            input_frame,
            values=["F=N+1", "F=N-", "F=N+", "F=N-1", "None"],
            width=6,
        )
        self.combo_component.set("F=N+1")
        self.combo_component.grid(row=0, column=7, padx=4, pady=2)

        # Row 4: Action Buttons
        btn_frame = ttk.Frame(self.control_frame)
        btn_frame.pack(fill=tk.X, padx=5, pady=5)

        self.btn_save = ttk.Button(
            btn_frame, text="✔ Save & Next Peak", command=self.save_assignment
        )
        self.btn_save.pack(side=tk.LEFT, padx=5)

        self.btn_add_extra = ttk.Button(
            btn_frame, text="➕ Add Extra Label", command=self.add_extra_label
        )
        self.btn_add_extra.pack(side=tk.LEFT, padx=5)

        self.btn_pass = ttk.Button(
            btn_frame, text="⏭ Pass / Skip Peak", command=self.pass_peak
        )
        self.btn_pass.pack(side=tk.LEFT, padx=5)

        self.btn_clear = ttk.Button(
            btn_frame, text="✖ Clear Saved Labels", command=self.clear_assignment
        )
        self.btn_clear.pack(side=tk.LEFT, padx=5)

        # Load CSV and Export CSV Buttons
        self.btn_export = ttk.Button(
            btn_frame,
            text="💾 Export Assignments to CSV",
            command=self.export_csv,
        )
        self.btn_export.pack(side=tk.RIGHT, padx=5)

        self.btn_load = ttk.Button(
            btn_frame,
            text="📂 Load Assignments from CSV",
            command=self.load_csv,
        )
        self.btn_load.pack(side=tk.RIGHT, padx=5)

    def _update_x_scroll_bounds(self):
        """Updates the slider range (X-Min and X-Max) dynamically from GUI entries."""
        try:
            new_min = float(self.ent_xmin.get())
            new_max = float(self.ent_xmax.get())

            if new_min < new_max:
                self.scroll_xmin = new_min
                self.scroll_xmax = new_max
                self.slider_x.configure(from_=new_min, to=new_max)

                cur_val = self.slider_x.get()
                clamped_val = max(new_min, min(new_max, cur_val))
                self._updating_slider = True
                self.slider_x.set(clamped_val)
                self._updating_slider = False

                self._on_slider_scroll(clamped_val)
            else:
                messagebox.showerror(
                    "Error", "X-Min must be strictly less than X-Max."
                )
        except ValueError:
            messagebox.showerror(
                "Error", "Invalid float entry for X scroll bounds."
            )

    def _update_label_layout(self):
        """Reads GUI parameters for vertical gap and collision radius, then redraws."""
        try:
            spacing_val = float(self.spin_v_spacing.get())
            radius_val = float(self.spin_collision.get())
            if spacing_val > 0 and radius_val > 0:
                self.label_spacing_pt = spacing_val
                self.collision_radius_ghz = radius_val
                self._plot_spectrum()
                self._load_current_peak_data()
        except ValueError:
            pass

    def _plot_spectrum(self):
        self.ax.clear()

        # 1. Measured spectrum trace
        self.ax.plot(
            self.freqs_ghz,
            self.intensities,
            color="gray",
            alpha=0.5,
            lw=1.0,
            label="Measured",
        )

        # 2. Overlay prediction sticks with vertical labels
        if self.prediction_sticks:
            max_meas = (
                np.nanmax(self.intensities) if len(self.intensities) > 0 else 1.0
            )
            max_pop = max(s["pop"] for s in self.prediction_sticks)
            scale = (max_meas / max_pop) * 0.9 if max_pop > 0 else 1.0

            cmap = plt.get_cmap("tab10")
            b_colors = {
                "O": cmap(0),
                "P": cmap(1),
                "Q": cmap(2),
                "R": cmap(3),
            }

            seen_branches = set()
            for stick in self.prediction_sticks:
                x_val = stick["x_offset"]
                y_val = max(stick["pop"] * scale, max_meas * 0.05)
                branch = stick["branch"]
                c = b_colors.get(branch, "darkred")
                ls = stick.get("linestyle", "solid")

                lbl = (
                    f"Branch {branch}" if branch not in seen_branches else None
                )
                if lbl:
                    seen_branches.add(branch)

                self.ax.vlines(
                    x=x_val,
                    ymin=0,
                    ymax=y_val,
                    color=c,
                    linestyles=ls,
                    alpha=0.55,
                    lw=1.1,
                    label=lbl,
                    clip_on=True,
                )

                # Vertical label positioning
                self.ax.text(
                    x_val,
                    y_val + (max_meas * 0.015),
                    stick["label"],
                    rotation=90,  # Rotates text vertically
                    ha="center",  # Center horizontal alignment on stick
                    va="bottom",  # Start text at stick tip
                    fontsize=7,
                    color=c,
                    alpha=0.8,
                    clip_on=True,
                )

        # 3. Detected peak points
        self.ax.plot(
            self.peak_positions_ghz,
            self.intensities[self.peak_indices],
            "r.",
            ms=5,
            alpha=0.6,
            label="Detected Peaks",
            clip_on=True,
        )

        # 4. Active Peak marker
        (self.current_marker,) = self.ax.plot(
            [], [], "go", ms=9, zorder=5, label="Active Peak", clip_on=True
        )

        # 5. Redraw saved annotations with DYNAMIC HEIGHT STAGGERING
        all_annotations = []
        for p_idx, assign_list in self.assignments.items():
            for i, assign in enumerate(assign_list):
                all_annotations.append({
                    "p_idx": p_idx,
                    "sub_idx": i,
                    "x_pos": assign["Frequency_Offset_GHz"],
                    "y_val": self.intensities[p_idx],
                    "label": assign["Label"],
                })

        all_annotations.sort(key=lambda item: item["x_pos"])
        occupied_tiers = []

        for item in all_annotations:
            x_pos = item["x_pos"]
            sub_idx = item["sub_idx"]

            nearby_tiers = [
                t
                for (x, t) in occupied_tiers
                if abs(x - x_pos) < self.collision_radius_ghz
            ]

            tier = 0
            while tier in nearby_tiers:
                tier += 1

            occupied_tiers.append((x_pos, tier))

            total_y_offset = (
                12
                + (sub_idx * self.label_spacing_pt)
                + (tier * self.label_spacing_pt)
            )

            arrow_dict = (
                dict(
                    arrowstyle="->",
                    color="blue",
                    lw=0.8,
                    alpha=0.7,
                    shrinkA=0,
                    shrinkB=2,
                )
                if sub_idx == 0
                else None
            )

            self.ax.annotate(
                item["label"],
                xy=(x_pos, item["y_val"]),
                xytext=(0, total_y_offset),
                textcoords="offset points",
                ha="center",
                fontsize=8,
                color="blue",
                fontweight="bold",
                arrowprops=arrow_dict,
                clip_on=True,
            )

        self.ax.set_xlabel(
            f"Frequency Offset (GHz) from {self.center_freq_thz:.6f} THz"
        )
        self.ax.set_ylabel("PMT Signal / Intensity")
        self.ax.grid(True, linestyle=":", alpha=0.5)

        # Legend with Isotope line styles
        handles, labels = self.ax.get_legend_handles_labels()
        iso_handles = [
            Line2D(
                [0],
                [0],
                color="gray",
                lw=1.2,
                linestyle="-",
                label="174YbF (Solid)",
            ),
            Line2D(
                [0],
                [0],
                color="gray",
                lw=1.2,
                linestyle="--",
                label="172YbF (Dashed)",
            ),
            Line2D(
                [0],
                [0],
                color="gray",
                lw=1.2,
                linestyle=":",
                label="176YbF (Dotted)",
            ),
        ]

        all_handles = handles + iso_handles

        self.ax.legend(
            handles=all_handles,
            title="Key",
            bbox_to_anchor=(1.01, 1.0),
            loc="upper left",
            borderaxespad=0.0,
            fontsize=8,
            title_fontsize=9,
            frameon=True,
        )

        self.fig.tight_layout(rect=[0, 0, 0.88, 1.0])

    def _load_current_peak_data(self):
        if self.num_peaks == 0:
            self.lbl_peak_info.config(text="No peaks detected.")
            return

        peak_idx = self.peak_indices[self.current_idx]
        freq_offset_ghz = self.peak_positions_ghz[self.current_idx]
        y_val = self.intensities[peak_idx]

        num_attached = len(self.assignments.get(peak_idx, []))
        label_count_str = (
            f" | ({num_attached} label{'s' if num_attached != 1 else ''})"
            if num_attached > 0
            else ""
        )

        self.lbl_peak_info.config(
            text=f"Peak {self.current_idx + 1} / {self.num_peaks} | Index: {peak_idx} | Offset: {freq_offset_ghz:+.4f} GHz{label_count_str}"
        )

        self.current_marker.set_data([freq_offset_ghz], [y_val])

        self.ax.set_xlim(
            freq_offset_ghz - self.zoom_window,
            freq_offset_ghz + self.zoom_window,
        )
        self.ax.set_ylim(self.y_min_global, self.y_max_global)

        self._updating_slider = True
        self.slider_x.set(freq_offset_ghz)
        self._updating_slider = False

        if num_attached > 0:
            last_data = self.assignments[peak_idx][-1]
            self.combo_isotope.set(last_data["Isotope"])
            self.combo_branch.set(last_data["Branch"])
            self.spin_n.set(last_data["N"])
            self.combo_component.set(last_data["Component"])

        self.canvas.draw_idle()

    def _on_slider_scroll(self, val):
        if self._updating_slider:
            return

        center_ghz = float(val)
        self.ax.set_xlim(
            center_ghz - self.zoom_window, center_ghz + self.zoom_window
        )
        self.ax.set_ylim(self.y_min_global, self.y_max_global)
        self.canvas.draw_idle()

    def _apply_manual_ymax(self):
        try:
            val = float(self.ent_ymax.get())
            if val > 0:
                self.y_max_global = val
                self._load_current_peak_data()
        except ValueError:
            pass

    def zoom_in(self):
        self.zoom_window = max(0.1, self.zoom_window / 1.5)
        self.spin_window.set(f"{self.zoom_window:.2f}")
        self._load_current_peak_data()

    def zoom_out(self):
        self.zoom_window = min(100.0, self.zoom_window * 1.5)
        self.spin_window.set(f"{self.zoom_window:.2f}")
        self._load_current_peak_data()

    def _update_zoom(self):
        try:
            val = float(self.spin_window.get())
            if val > 0:
                self.zoom_window = val
                self._load_current_peak_data()
        except ValueError:
            pass

    def recenter_frequency(self):
        try:
            new_center = float(self.ent_center_freq.get())
            if self.frequencies_in_thz:
                raw_freqs = (self.freqs_ghz / 1000.0) + self.center_freq_thz
                raw_peaks = (
                    self.peak_positions_ghz / 1000.0
                ) + self.center_freq_thz

                self.center_freq_thz = new_center
                self.freqs_ghz = (raw_freqs - self.center_freq_thz) * 1000.0
                self.peak_positions_ghz = (
                    raw_peaks - self.center_freq_thz
                ) * 1000.0

                self._prepare_predictions()
                self._plot_spectrum()
                self._load_current_peak_data()
        except ValueError:
            messagebox.showerror(
                "Error", "Invalid Center Frequency float value."
            )

    def prev_peak(self):
        if self.current_idx > 0:
            self.current_idx -= 1
            self._load_current_peak_data()

    def next_peak(self):
        if self.current_idx < self.num_peaks - 1:
            self.current_idx += 1
            self._load_current_peak_data()

    def pass_peak(self):
        self.next_peak()

    def _get_current_assignment_dict(self):
        peak_idx = self.peak_indices[self.current_idx]
        freq_pos_ghz = self.peak_positions_ghz[self.current_idx]

        isotope = self.combo_isotope.get()
        branch = self.combo_branch.get()
        n_val = int(self.spin_n.get())
        comp = self.combo_component.get()

        label_str = f"{isotope} {branch}({n_val})" + (
            f" {comp}" if comp != "None" else ""
        )
        abs_freq_thz = (freq_pos_ghz / 1000.0) + self.center_freq_thz

        return {
            "Peak_Index": peak_idx,
            "Frequency_Offset_GHz": freq_pos_ghz,
            "Absolute_Freq_THz": abs_freq_thz,
            "Isotope": isotope,
            "Branch": branch,
            "N": n_val,
            "Component": comp,
            "Label": label_str,
        }

    def add_extra_label(self):
        peak_idx = self.peak_indices[self.current_idx]
        new_data = self._get_current_assignment_dict()

        if peak_idx not in self.assignments:
            self.assignments[peak_idx] = []

        if new_data not in self.assignments[peak_idx]:
            self.assignments[peak_idx].append(new_data)

        self._plot_spectrum()
        self._load_current_peak_data()

    def save_assignment(self):
        peak_idx = self.peak_indices[self.current_idx]
        new_data = self._get_current_assignment_dict()

        if peak_idx not in self.assignments:
            self.assignments[peak_idx] = []

        if new_data not in self.assignments[peak_idx]:
            self.assignments[peak_idx].append(new_data)

        self._plot_spectrum()
        self.next_peak()

    def clear_assignment(self):
        peak_idx = self.peak_indices[self.current_idx]
        if peak_idx in self.assignments:
            del self.assignments[peak_idx]
            self._plot_spectrum()
            self._load_current_peak_data()

    def get_assignments_dict(self):
        return self.assignments

    def get_assignments_dataframe(self):
        if not self.assignments:
            return pd.DataFrame()

        all_rows = [
            item
            for assign_list in self.assignments.values()
            for item in assign_list
        ]
        df = pd.DataFrame(all_rows)
        return df.sort_values(by="Frequency_Offset_GHz").reset_index(drop=True)

    def export_csv(self):
        df = self.get_assignments_dataframe()
        if df.empty:
            messagebox.showwarning("Warning", "No peak assignments logged yet!")
            return

        file_path = filedialog.asksaveasfilename(
            defaultextension=".csv",
            filetypes=[("CSV files", "*.csv"), ("All files", "*.*")],
            title="Save Assigned Peaks Dataset",
        )
        if file_path:
            df.to_csv(file_path, index=False)
            messagebox.showinfo(
                "Export Successful",
                f"Saved {len(df)} peak assignments to:\n{file_path}",
            )

    def load_csv(self):
        file_path = filedialog.askopenfilename(
            defaultextension=".csv",
            filetypes=[("CSV files", "*.csv"), ("All files", "*.*")],
            title="Load Assigned Peaks Dataset",
        )
        if not file_path:
            return

        try:
            df = pd.read_csv(file_path)
            required_cols = {
                "Peak_Index",
                "Frequency_Offset_GHz",
                "Absolute_Freq_THz",
                "Isotope",
                "Branch",
                "N",
                "Component",
                "Label",
            }
            if not required_cols.issubset(df.columns):
                messagebox.showerror(
                    "Error",
                    r"CSV file is missing required columns. Expected format from previous exports.",
                )
                return

            new_assignments = {}
            for _, row in df.iterrows():
                p_idx = int(row["Peak_Index"])
                item = {
                    "Peak_Index": p_idx,
                    "Frequency_Offset_GHz": float(row["Frequency_Offset_GHz"]),
                    "Absolute_Freq_THz": float(row["Absolute_Freq_THz"]),
                    "Isotope": str(row["Isotope"]),
                    "Branch": str(row["Branch"]),
                    "N": int(row["N"]),
                    "Component": str(row["Component"]),
                    "Label": str(row["Label"]),
                }
                if p_idx not in new_assignments:
                    new_assignments[p_idx] = []
                new_assignments[p_idx].append(item)

            self.assignments = new_assignments
            self._plot_spectrum()
            self._load_current_peak_data()
            messagebox.showinfo(
                "Load Successful",
                f"Loaded {len(df)} assignment rows from:\n{file_path}",
            )

        except Exception as e:
            messagebox.showerror("Error", f"Failed to load CSV file:\n{e}")


def launch_peak_assigner(
    frequencies,
    intensities,
    peak_indices,
    peak_positions,
    center_freq_thz=568.681,
    dfs_by_isotope=None,
    stick_offset_ghz=-3.15,
    x_branches_hyperfine=None,
    zoom_window_ghz=1.5,
    x_min_ghz=None,
    x_max_ghz=None,
):
    root = tk.Tk()
    app = PeakAssignerGUI(
        root=root,
        frequencies=frequencies,
        intensities=intensities,
        peak_indices=peak_indices,
        peak_positions=peak_positions,
        center_freq_thz=center_freq_thz,
        dfs_by_isotope=dfs_by_isotope,
        stick_offset_ghz=stick_offset_ghz,
        x_branches_hyperfine=x_branches_hyperfine,
        zoom_window_ghz=zoom_window_ghz,
        x_min_ghz=x_min_ghz,
        x_max_ghz=x_max_ghz,
    )
    root.mainloop()

    return app.get_assignments_dataframe()