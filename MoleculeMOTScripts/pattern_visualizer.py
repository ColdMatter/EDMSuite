#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Created on Tue Mar 24 06:15:22 2020

@author: arijit
"""

import numpy as np
import tkinter as tk
import os
import json
import zipfile

from matplotlib.backends.backend_tkagg import (
    FigureCanvasTkAgg, NavigationToolbar2Tk)
from matplotlib.backend_bases import key_press_handler
from matplotlib.figure import Figure


class GUI():
    def __init__(self):
        self.root = tk.Tk()
        self.root.wm_title("Pattern visualizer")

        self.filename = tk.StringVar()
        self.filename.set(os.path.abspath('.'))

        self.chanel_list = ["Select"]
        self.index = 0
        self.opt_menus = []
        self.opt_vars = []
        self.gain_vars = []
        self.offset_vars = []
        self.color_list = ['red', 'blue', 'green',
                           'black', 'cyan', 'magenta',
                           'yellow']
        self.color_vars = []
        for color in 5*self.color_list:
            color_var = tk.StringVar()
            color_var.set(color)
            self.color_vars.append(color_var)

        self.window_height = 800
        self.sidebar_width = 400
        self.plotarea_width = 800

        self.root.geometry("{}x{}".format(self.sidebar_width
                           + self.plotarea_width, self.window_height))

        self._create_layout()
        self._initialize_plot_area()

    def _create_layout(self):
        self.sidebar = tk.Frame(
                self.root,
                bg='white',
                width=self.sidebar_width,
                height=self.window_height,
                relief='sunken',
                borderwidth=2)

        self.plotarea = tk.Frame(
                self.root,
                bg='white',
                width=self.plotarea_width,
                height=self.window_height)

        self.entry_with_button = tk.Frame(
                self.sidebar,
                bg='white',
                width=self.sidebar_width,
                height=10)

        self.entry = tk.Entry(
                self.entry_with_button,
                width=42,
                textvariable=self.filename)

        self.file_load_button = tk.Button(
                self.entry_with_button,
                text="Load file",
                command=self._load_file_callback,
                width=10)

        self.channel_add_button = tk.Button(
                self.sidebar,
                text="Add Channel",
                command=self._add_channel_callback,
                width=10,
                state=tk.DISABLED)

        self.plot_button = tk.Button(
                self.sidebar,
                text="Plot",
                command=self._plot_callback,
                width=10,
                state=tk.DISABLED)

        self.sidebar.pack(
                expand=True,
                fill='both',
                side='right')
        self.sidebar.pack_propagate(0)

        self.entry_with_button.place(relx=0.0, rely=0.0)
        self.entry.pack(side="left", fill="both")
        self.file_load_button.pack(side="right", fill="both")

        self.channel_add_button.place(relx=0.0, rely=0.05)
        self.plot_button.place(relx=0.75, rely=0.05)

        self.plotarea.pack(
                expand=True,
                fill='both',
                side='left')
        self.plotarea.pack_propagate(0)

        self._add_channel_callback()

    def _add_channel_callback(self):
        self.index += 1
        self.opt_vars.append(tk.StringVar())
        self.opt_vars[self.index-1].set(self.chanel_list[0])

        self.gain_vars.append(tk.StringVar())
        self.gain_vars[self.index-1].set(1.0)

        self.offset_vars.append(tk.StringVar())
        self.offset_vars[self.index-1].set(0.0)

        self._create_channel(rely=0.1*self.index)

        if self.index > 1:
            self.update_channel_options(self.index-1)

    def _create_channel(self, rely):
        ch = tk.Frame(
                self.sidebar,
                bg='white',
                width=self.sidebar_width,
                height=100)

        ch_label = tk.Label(
                ch,
                bg='white',
                justify=tk.LEFT,
                padx=0,
                pady=8,
                text='CH{}'.format(self.index))

        ch_opt_menu = tk.OptionMenu(
                ch,
                self.opt_vars[self.index-1],
                *self.chanel_list)
        ch_opt_menu.config(width=15)

        ch_offset_label = tk.Label(
                ch,
                bg='white',
                justify=tk.LEFT,
                padx=0,
                pady=0,
                text='offset')

        ch_offset = tk.Entry(
                ch,
                width=20,
                textvariable=self.offset_vars[self.index-1])

        ch_gain_label = tk.Label(
                ch,
                bg='white',
                justify=tk.LEFT,
                padx=0,
                pady=0,
                text='gain')

        ch_gain = tk.Entry(
                ch,
                width=20,
                textvariable=self.gain_vars[self.index-1])

        ch_color_menu = tk.OptionMenu(
                ch,
                self.color_vars[self.index-1],
                *self.color_list)
        ch_color_menu.config(width=10)

        ch.place(relx=0.0, rely=rely)
        ch_label.place(relx=0.1, rely=0.0)
        ch_opt_menu.place(relx=0.3, rely=0.0)
        ch_offset_label.place(relx=0.1, rely=0.3)
        ch_offset.place(relx=0.3, rely=0.3)
        ch_gain_label.place(relx=0.1, rely=0.5)
        ch_gain.place(relx=0.3, rely=0.5)
        ch_color_menu.place(relx=0.7, rely=0.0)

        self.opt_menus.append(ch_opt_menu)

    def _load_content_from_zip_file(self, filename):
        filebasename = os.path.basename(filename).split('.')[0]
        src = zipfile.ZipFile(filename, 'r')
        analog_dict = json.loads(
            src.read(filebasename+'_analogPattern.json'))
        digital_dict = json.loads(
            src.read(filebasename+'_digitalPattern.json'))
        return analog_dict, digital_dict

    def _load_file_callback(self):
        filename = tk.filedialog.askopenfilename(
                initialdir=".",
                title="Select file",
                filetypes=(("zip files", "*.zip"), ("all files", "*.*")))
        self.filename.set(str(filename))
        analog_dict, digital_dict = self._load_content_from_zip_file(filename)
        sorted_analog_dict, self.tmax, self.plottable_keys = \
            self.analog_sort(analog_dict)
        self.plottable_dict = self.analog_fill(sorted_analog_dict)

        filled_sorted_digital_dict = self.digital_sort_fill(digital_dict)
        self.plottable_dict.update(filled_sorted_digital_dict)

        self.channel_add_button['state'] = 'normal'
        self.plot_button['state'] = 'normal'
        self.update_channel_options(0)

    def analog_sort(self, in_dict):
        out_dict = {}
        tmax = 0
        key_list = []
        for key, val in in_dict.items():
            out_dict[key] = {}
            out_dict[key]['t'] = np.array(list(val.keys()), dtype=np.int)
            out_dict[key]['V'] = np.array(list(val.values()), dtype=np.float)
            tmax = np.max([tmax, np.max(out_dict[key]['t'])])
            if len(out_dict[key]['t']) > 2:
                sorted_list = out_dict[key]['t'].argsort()
                out_dict[key]['t'] = out_dict[key]['t'][sorted_list]
                out_dict[key]['V'] = out_dict[key]['V'][sorted_list]
                key_list.append(key)
        return out_dict, tmax, key_list

    def analog_fill(self, in_dict):
        out_dict = {}
        for key in self.plottable_keys:
            dict_t = in_dict[key]['t']
            dict_V = in_dict[key]['V']
            filled_dict_t = []
            filled_dict_V = []
            for i in range(len(dict_t)-1):
                dt = dict_t[i+1]-dict_t[i]
                if dt > 1:
                    filled_dict_t.append(dict_t[i])
                    filled_dict_V.append(dict_V[i])
                    if dict_V[i] < dict_V[i+1]:
                        filled_dict_t.append(dict_t[i+1])
                        filled_dict_V.append(dict_V[i])
                    else:
                        filled_dict_t.append(dict_t[i])
                        filled_dict_V.append(dict_V[i+1])
                else:
                    filled_dict_t.append(dict_t[i])
                    filled_dict_V.append(dict_V[i])
            filled_dict_t.append(self.tmax)
            filled_dict_V.append(dict_V[-1])
            out_dict[key] = {'t': np.array(filled_dict_t),
                             'V': np.array(filled_dict_V)}
        return out_dict

    def digital_sort_fill(self, in_dict):
        pattern = in_dict['pattern']
        channels = in_dict['channels']
        line_split_list = pattern.split('\n')[1:-1]
        ttime = []
        out_dict = {}
        for key, val in channels.items():
            out_dict[key] = []
        for i, item in enumerate(line_split_list):
            tab_split_list = item.split('\t')
            ttime.append(tab_split_list[0])
            items = tab_split_list[1:-1]
            for key, val in channels.items():
                out_dict[key].append(items[val])
        for key, val in out_dict.items():
            new_val = []
            tall = []
            for i, t in enumerate(ttime):
                if i == 0:
                    if val[i] == 'U':
                        tall.append(t)
                        new_val.append(0)
                        tall.append(t)
                        new_val.append(5)
                    elif val[i] == '-':
                        tall.append(t)
                        new_val.append(0)
                else:
                    if val[i] == 'U':
                        tall.append(t)
                        new_val.append(0)
                        tall.append(t)
                        new_val.append(5)
                    elif val[i] == 'D':
                        tall.append(t)
                        new_val.append(5)
                        tall.append(t)
                        new_val.append(0)
                    elif val[i] == '-' and val[i-1] in ['-', 'D']:
                        tall.append(t)
                        new_val.append(0)
                    elif val[i] == '-' and val[i-1] == 'U':
                        tall.append(t)
                        new_val.append(5)
                        val[i] = 'U'
            out_dict[key] = [tall, new_val]

        new_out_dict = {}
        for key, val in out_dict.items():
            new_out_dict[key] = {'t': np.array(val[0], dtype=np.int),
                                 'V': np.array(val[1], dtype=np.float)}
        return new_out_dict

    def update_channel_options(self, index):
        menu = self.opt_menus[index]["menu"]
        var = self.opt_vars[index]
        menu.delete(0, "end")
        for string in self.plottable_dict.keys():
            menu.add_command(label=string,
                             command=lambda value=string: var.set(value))

    def _initialize_plot_area(self):
        self.fig = Figure(figsize=(7, 7), dpi=100)
        self.ax = self.fig.add_subplot(111)
        self.ax.set_xlabel('time axis')
        self.ax.set_ylabel('voltage')

        self.canvas = FigureCanvasTkAgg(self.fig, master=self.plotarea)
        self.canvas.draw()
        self.canvas.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH)

        self.toolbar = NavigationToolbar2Tk(self.canvas, self.plotarea)
        self.toolbar.update()
        self.canvas.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH)

        self.canvas.mpl_connect("key_press_event", self.on_key_press)

    def _plot_callback(self):
        self.ax.cla()
        for i in range(self.index):
            offset = np.float(self.offset_vars[i].get())
            gain = np.float(self.gain_vars[i].get())
            opt = str(self.opt_vars[i].get())
            color = str(self.color_vars[i].get())
            if opt != 'Select':
                xaxis = self.plottable_dict[opt]['t']
                yaxis = offset + gain*self.plottable_dict[opt]['V']
                self.ax.plot(xaxis, yaxis, color)
                self.ax.set_xlabel('time axis')
                self.ax.set_ylabel('voltage')
                self.canvas.draw()

    def on_key_press(self, event):
        key_press_handler(event, self.canvas, self.toolbar)

    def run(self):
        self.root.mainloop()


if __name__ == '__main__':
    gui = GUI()
    gui.run()
