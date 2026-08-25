List of Tables
List of Tables
Table 1: Symbols and Safety Labels........................................................................................................................................... 10
Table 2: Packing List M4i card (PCI Express)............................................................................................................................... 13
Table 3: Packing List M4x card (PXI Express)............................................................................................................................... 13
Table 4: Feature and firmware matrix of 66xx and 96xx products................................................................................................. 16
Table 5: list of C/C++ header files in driver................................................................................................................................ 62
Table 6: C/C++ type declarations for drivers and examples......................................................................................................... 64
Table 7: C/C++ type naming convention throughout drivers and examples..................................................................................... 64
Table 8: Spectrum driver API functions overview and differentiation between 32 bit and 64 bit registers............................................ 66
Table 9: Spectrum API: Command register and basic commands................................................................................................... 79
Table 10: Spectrum API: Card Type Register............................................................................................................................... 82
Table 11: Spectrum API: list of card type codes for M4i.66xx series............................................................................................... 82
Table 12: Spectrum API: list of card type codes for M4x.66xx series.............................................................................................. 82
Table 13: Spectrum API: list of card type codes for M4i.96xx series............................................................................................... 82
Table 14: Spectrum API: list of card type codes for M4x.96xx series.............................................................................................. 82
Table 15: Spectrum API: hardware and PCB version register overview........................................................................................... 83
Table 16: Spectrum API: extension module hardware and PCB version register............................................................................... 83
Table 17: Spectrum API: register for reading back the PXIe card slot number................................................................................... 83
Table 18: Spectrum API: Register overview of firmware versions.................................................................................................... 83
Table 19: Spectrum API: Register overview of reading current firmware.......................................................................................... 84
Table 20: Spectrum API: production date register......................................................................................................................... 84
Table 21: Spectrum API: calibration date register......................................................................................................................... 84
Table 22: Spectrum API: hardware serial number register............................................................................................................. 84
Table 23: Spectrum API: maximum sampling rate register............................................................................................................. 85
Table 24: Spectrum API: installed memory registers...................................................................................................................... 85
Table 25: Spectrum API: Feature Register and available feature flags............................................................................................. 85
Table 26: Spectrum API: Extended feature register and available extended feature flags.................................................................. 86
Table 27: Spectrum API: register overview of miscellaneous cards information................................................................................. 86
Table 28: Spectrum API: register card function type and possible types.......................................................................................... 86
Table 29: Spectrum API: register driver type information and possible driver types........................................................................... 87
Table 30: Spectrum API: driver version read register.................................................................................................................... 87
Table 31: Spectrum API: kernel driver version read register........................................................................................................... 87
Table 32: Spectrum API: custom modification register and different bitmasks to split the register in various hardware parts................... 87
Table 33: Spectrum API: command register and reset command.................................................................................................... 88
Table 34: Spectrum API: channel enable register and register settings............................................................................................ 89
Table 35: Spectrum API: channel count register........................................................................................................................... 89
Table 36: Spectrum API: output enable register and register settings............................................................................................... 90
Table 37: Spectrum API: output amplitude registers and register settings depending on board type.................................................... 90
Table 38: Spectrum API: output amplitude minimum and maximum values registers.......................................................................... 91
Table 39: Spectrum API: output filter registers and register settings................................................................................................. 91
Table 40: output filter specifications depending on card version.................................................................................................... 91
Table 41: Spectrum API: differential output register and register settings......................................................................................... 92
Table 42: availability of differential output mode depending on AWG model................................................................................. 92
Table 43: Spectrum API: double output mode registers................................................................................................................. 93
Table 44: availability of double output mode depending on AWG model....................................................................................... 93
Table 45: Spectrum API: stop level register and register settings..................................................................................................... 93
Table 46: Spectrum API: custom stop level registers...................................................................................................................... 93
Table 47: Spectrum API: reading out the available features of the analog outputs............................................................................ 94
Table 48: Spectrum API: card mode and read out of available card mode software registers............................................................ 95
Table 49: Spectrum API: card command register and different commands with descriptions.............................................................. 96
Table 50: Spectrum API: timeout definition register....................................................................................................................... 96
Table 51: Spectrum API: card status register and possible status values with descriptions of the status................................................ 97
Table 52: Spectrum API: memory test register.............................................................................................................................. 99
Table 53: Spectrum API: Command register and commands for DMA transfers................................................................................ 99
Table 54: Spectrum API: status register and status codes for DMA data transfer............................................................................. 100
Table 55: Spectrum API: card mode register and single mode settings.......................................................................................... 100
Table 56: Spectrum API: memory and loop settings.................................................................................................................... 101
Table 57: Spectrum API: overview of mode settings in relation to loops settings and resulting behaviour........................................... 102
Table 58: Spectrum API: FIFO single replay mode register and settings........................................................................................ 103
Table 59: Spectrum API: FIFO mode length settings registers....................................................................................................... 103
Table 60: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode................................. 105
Table 61: Spectrum API: registers for DMA buffer handling......................................................................................................... 106
Table 62: Spectrum API: content of DMA buffer handling registers for different use cases............................................................... 106
Table 63: Spectrum API: output buffer size register and register settings........................................................................................ 109
Table 64: output latency depending on channel settings, buffer settings and output FIFO................................................................ 110
Table 65: M4i and M4x cards data organization...................................................................................................................... 111
Table 66: Spectrum API: data format and DAC resolution depending on selected mode and digital output modes............................. 111
Table 67: Spectrum API: hardware data conversion registers and available register settings........................................................... 111
Table 68: Spectrum API: clock mode register and available clock modes...................................................................................... 113
Table 69: Spectrum API: samplerate register............................................................................................................................. 114
Table 70: Spectrum API: clock mode register and internal clock mode.......................................................................................... 114
(c) Spectrum Instrumentation GmbH 212

List of Tables
Table 71: Spectrum API: clock output and clock output frequency register..................................................................................... 114
Table 72: Spectrum API: clock mode register and quartz 2 settings.............................................................................................. 114
Table 73: Spectrum API: clock output and clock output frequency register..................................................................................... 115
Table 74: Spectrum API: clock mode register and external reference clock setup............................................................................ 115
Table 75: Spectrum API: reference clock register and available settings........................................................................................ 115
Table 76: Spectrum API: clock output and clock output frequency register..................................................................................... 116
Table 77: Spectrum API: clock mode register and PXI reference clock usage................................................................................. 116
Table 78: Spectrum API: general trigger OR mask register and available settings.......................................................................... 118
Table 79: Spectrum API: channel trigger OR mask registers and available settings......................................................................... 119
Table 80: Spectrum API: general trigger AND mask registers and available settings...................................................................... 120
Table 81: Spectrum API: channel trigger AND mask registers and available settings...................................................................... 120
Table 82: Spectrum API: software register and register setting for software trigger......................................................................... 121
Table 83: Spectrum API: command register and force trigger command....................................................................................... 121
Table 84: Spectrum API: command register and trigger enable/disable command......................................................................... 121
Table 85: Spectrum API: trigger delay registers and available settings.......................................................................................... 122
Table 86: Spectrum API: trigger counter register and register return values.................................................................................... 122
Table 87: Spectrum API: external trigger Ext0 registers and register settings.................................................................................. 123
Table 88: Spectrum API: external trigger Ext0 OR mask settings................................................................................................... 123
Table 89: Spectrum API: external trigger Ext0 input termination................................................................................................... 123
Table 90: Spectrum API: external trigger Ext0 input coupling....................................................................................................... 124
Table 91: Spectrum API: external trigger Ext1 registers and register settings.................................................................................. 124
Table 92: Spectrum API: external trigger Ext1 OR mask settings................................................................................................... 124
Table 93: Spectrum API: external trigger available settings for trigger levels.................................................................................. 124
Table 94: Spectrum API: external trigger OR mask and AND mask register and settings................................................................. 125
Table 95: Spectrum API: external register mode setup for trigger on positive edge......................................................................... 125
Table 96: Spectrum API: external register mode setup for trigger on negative edge........................................................................ 125
Table 97: Spectrum API: external trigger register mode setup for trigger on positive and negative edge........................................... 126
Table 98: Spectrum API: external trigger register mode setup for trigger re-arm on positive edge..................................................... 126
Table 99: Spectrum API: external trigger register mode setup for trigger re-arm on negative edge.................................................... 126
Table 100: Spectrum API: external trigger register mode setup for window trigger for entering signals............................................. 127
Table 101: Spectrum API: external trigger register mode setup for window trigger for leaving signals.............................................. 127
Table 102: Spectrum API: external trigger register mode setup for high level trigger....................................................................... 127
Table 103: Spectrum API: external trigger register mode setup for low level trigger........................................................................ 128
Table 104: Spectrum API: external trigger register mode setup for in window trigger..................................................................... 128
Table 105: Spectrum API: external trigger register mode setup for outside window trigger.............................................................. 128
Table 106: trigger overview with PXI trigger lines marked........................................................................................................... 129
Table 107: Spectrum API: PXI trigger register and available register settings................................................................................. 129
Table 108: Spectrum API: PXI trigger mask register and available register settings......................................................................... 130
Table 109: Spectrum API: multi-purpose I/O lines registers and available register settings.............................................................. 132
Table 110: Spectrum API: asynchronous I/O register settings of the multi-purpose I/O registers...................................................... 133
Table 111: Spectrum API: additional trigger output register for compatibility with older hardware................................................... 133
Table 112: Spectrum API: multi-purpose I/O registers and synchronous digital output settings......................................................... 134
Table 113: Spectrum API: data format and DAC resolution depending on selected mode and digital output modes........................... 134
Table 114: Spectrum API: segment size register for multiple replay mode..................................................................................... 136
Table 115: Spectrum API: card mode register and multiple replay settings.................................................................................... 137
Table 116: Spectrum API: memory and loop registers with related multiple replay settings.............................................................. 137
Table 117: Spectrum API: loops register settings when using Multiple Replay FIFO mode................................................................ 137
Table 118: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode............................... 138
Table 119: Spectrum API: stop level register and register settings................................................................................................. 139
Table 120: Spectrum API: custom stop level registers.................................................................................................................. 139
Table 121: Spectrum API: card mode register and settings for Gated Replay standard mode.......................................................... 140
Table 122: Spectrum API: memsize and loops register and register settings for Gated Replay mode................................................ 140
Table 123: Spectrum API: card mode register and Gated Replay FIFO mode settings..................................................................... 141
Table 124: Spectrum API: Gated Replay FIFO mode loops register settings................................................................................... 141
Table 125: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode............................... 141
Table 126: Spectrum API: trigger mask registers and available register settings............................................................................. 142
Table 127: Spectrum API: trigger register settings for trigger on positive edge............................................................................... 142
Table 128: Spectrum API: trigger register settings for trigger on negative edge.............................................................................. 143
Table 129: Spectrum API: trigger register settings for re-arm trigger on positive edge..................................................................... 144
Table 130: Spectrum API: trigger register settings for re-arm trigger on negative edge.................................................................... 144
Table 131: Spectrum API: trigger register settings for window trigger on entering signals............................................................... 144
Table 132: Spectrum API: trigger register settings for window trigger on leaving signals................................................................. 145
Table 133: Spectrum API: trigger register settings for high-level trigger......................................................................................... 145
Table 134: Spectrum API: trigger register settings for low-level trigger.......................................................................................... 145
Table 135: Spectrum API: trigger register settings for in-window trigger........................................................................................ 146
Table 136: Spectrum API: trigger register settings for outside-window trigger................................................................................ 146
Table 137: Spectrum API: stop level register and register settings................................................................................................. 147
Table 138: Spectrum API: custom stop level registers.................................................................................................................. 147
Table 139: Spectrum API: sequence mode registers and register settings...................................................................................... 149
Table 140: Spectrum API: card mode register with Sequence Mode setup.................................................................................... 149
Table 141: Spectrum API: sequence mode registers for segment handling..................................................................................... 149
Table 142: Spectrum API: sequence mode step registers and register setup................................................................................... 150
(c) Spectrum Instrumentation GmbH 213

List of Tables
Table 143: Spectrum API: sequence mode start register.............................................................................................................. 150
Table 144: Spectrum API: sequence mode segment status register................................................................................................ 151
Table 145: Spectrum API: stop level register and register settings................................................................................................. 151
Table 146: Spectrum API: custom stop level registers.................................................................................................................. 151
Table 147: Spectrum API: card mode and read out of available card mode software registers........................................................ 155
Table 148: Spectrum API: DDS information on the command queue............................................................................................. 157
Table 149: Spectrum API: DDS command register...................................................................................................................... 157
Table 150: Spectrum API: DDS trigger sources.......................................................................................................................... 157
Table 151: Spectrum API: DDS trigger status register.................................................................................................................. 158
Table 152: Spectrum API: DDS information registers.................................................................................................................. 158
Table 153: Spectrum API: DDS trigger timer.............................................................................................................................. 158
Table 154: Spectrum API: DDS20 connection register................................................................................................................ 159
Table 155: Spectrum API: DDS core frequency settings............................................................................................................... 160
Table 156: Spectrum API: DDS programmable frequency range.................................................................................................. 160
Table 157: Spectrum API: DDS core amplitude settings............................................................................................................... 160
Table 158: Spectrum API: DDS programmable amplitude range.................................................................................................. 161
Table 159: Spectrum API: DDS core phase settings.................................................................................................................... 161
Table 160: Spectrum API: DDS programmable phase range....................................................................................................... 161
Table 161: Spectrum API: DDS core frequency slope settings...................................................................................................... 162
Table 162: Spectrum API: DDS programmable frequency slope range.......................................................................................... 162
Table 163: Spectrum API: DDS core amplitude slope settings...................................................................................................... 162
Table 164: Spectrum API: DDS programmable amplitude slope range.......................................................................................... 163
Table 165: Spectrum API: DDS core slope step size settings........................................................................................................ 163
Table 166: Spectrum API: DDS Phase Behaviour........................................................................................................................ 164
Table 167: Spectrum API: multi-purpose I/O lines registers and available register settings.............................................................. 165
Table 168: Spectrum API: DDS multi-purpose I/O additional registers.......................................................................................... 165
Table 169: Spectrum API: DDS multi-purpose I/O manual output register...................................................................................... 165
Table 170: Spectrum API: register lists register.......................................................................................................................... 166
Table 171: Spectrum API: data transfer mode definition............................................................................................................. 167
Table 172: Comparison of the different DDS FIFOs and their fillsize registers................................................................................ 167
Table 173: Comparison of single and DMA transfer mode.......................................................................................................... 168
Table 174: Spectrum API: card mode and read out of available card mode software registers........................................................ 169
Table 175: Spectrum API: DDS information on the command queue............................................................................................. 171
Table 176: Spectrum API: DDS command register...................................................................................................................... 171
Table 177: Spectrum API: DDS trigger sources.......................................................................................................................... 171
Table 178: Spectrum API: DDS trigger status register.................................................................................................................. 172
Table 179: Spectrum API: DDS information registers.................................................................................................................. 172
Table 180: Spectrum API: DDS trigger timer.............................................................................................................................. 172
Table 181: Spectrum API: DDS50 connection register................................................................................................................ 173
Table 182: Spectrum API: DDS core frequency settings............................................................................................................... 174
Table 183: Spectrum API: DDS programmable frequency range.................................................................................................. 174
Table 184: Spectrum API: DDS core amplitude settings............................................................................................................... 174
Table 185: Spectrum API: DDS programmable amplitude range.................................................................................................. 175
Table 186: Spectrum API: DDS core phase settings.................................................................................................................... 175
Table 187: Spectrum API: DDS programmable phase range....................................................................................................... 175
Table 188: Spectrum API: DDS core frequency slope settings...................................................................................................... 176
Table 189: Spectrum API: DDS programmable frequency slope range.......................................................................................... 176
Table 190: Spectrum API: DDS core amplitude slope settings...................................................................................................... 176
Table 191: Spectrum API: DDS programmable amplitude slope range.......................................................................................... 177
Table 192: Spectrum API: DDS core slope step size settings........................................................................................................ 177
Table 193: Spectrum API: DDS Phase Behaviour........................................................................................................................ 178
Table 194: Spectrum API: multi-purpose I/O lines registers and available register settings.............................................................. 179
Table 195: Spectrum API: DDS multi-purpose I/O additional registers.......................................................................................... 179
Table 196: Spectrum API: DDS multi-purpose I/O manual output register...................................................................................... 179
Table 197: Spectrum API: register lists register.......................................................................................................................... 180
Table 198: Spectrum API: data transfer mode definition............................................................................................................. 181
Table 199: Comparison of the different DDS FIFOs and their fillsize registers................................................................................ 181
Table 200: Comparison of single and DMA transfer mode.......................................................................................................... 182
Table 201: Spectrum API: pulse generator clock frequency read register...................................................................................... 184
Table 202: Spectrum API: pulse generator enable registers......................................................................................................... 185
Table 203: Spectrum API: pulse generator length/period register................................................................................................ 185
Table 204: Spectrum API: pulse generator HIGH time registers.................................................................................................... 185
Table 205: Spectrum API: pulse generator loops/pulse repetition registers.................................................................................... 186
Table 206: Spectrum API: pulse generator delay/phase shift registers.......................................................................................... 186
Table 207: Spectrum API: pulse generator mode registers with their available settings.................................................................... 186
Table 208: Spectrum API: pulse generator trigger MUX1 registers with their available settings........................................................ 187
Table 209: Spectrum API: pulse generator trigger MUX2 registers with their available settings........................................................ 188
Table 210: Spectrum API: pulse generator command register for trigger forcing by software........................................................... 188
Table 211: Spectrum API: pulse generator additional configuration registers with the available settings............................................ 188
Table 212: Spectrum API: XIO lines and mode software registers with their reduced to the settings required for outputting pulses........ 189
Table 213: star-hub clock overview diagram............................................................................................................................. 191
Table 214: Spectrum API: star-hub related registers for reading detected connections..................................................................... 192
(c) Spectrum Instrumentation GmbH 214

List of Tables
Table 215: Spectrum API: synchronization enable mask register.................................................................................................. 193
Table 216: Spectrum API: star-hub synchronization commands.................................................................................................... 194
Table 217: Spectrum API: clock mode register and settings for SH Direct mode............................................................................. 195
Table 218: Spectrum API: driver error codes and error description.............................................................................................. 200
Table 219: Spectrum API: temperature read-out registers of internal temperature sensors................................................................ 202
Table 220: Spectrum API: temperature limits............................................................................................................................. 202
Table 221: card status LED color and blink coding..................................................................................................................... 205
Table 222: Spectrum API: card identification LED register........................................................................................................... 205
Table 223: Abbreviations used throughout the Spectrum documents............................................................................................. 209
(c) Spectrum Instrumentation GmbH 215