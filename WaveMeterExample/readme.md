# Instruction to installing wavemeterlock server and wavemeter lock.

---
---

## Wavemeter lock is a simple way to lock your laser. The server broadcast the measured frequency, the wavemeter lock reads it and send a feedback to the laser. 
## To install them on your computer, follow the instructions bellow.

## 1. In DAQ/EnvironHelper.cs, find the config of the server computer, add the TCP channel number for the server to broadcast the measurement
##		serverTCPChannel = 0000;
## Then add the TCP channel number which lets wavemeter lock talks to ScanMaster
##		wavemeterLockTCPChannel = 0000;
##		Of course replace 0000 with your favourite four digit integer. The wavemeterlock server setup is done.

## 2. In DAQ/YourHardware.cs, config the analog output to the laser(s). 
##		wmlConfig.AddSlaveLaser("SlaveLaser1", "WavemeterLock1",6);//Laser name, analog channel, wavemeter channel
##		Refer to DAQ/WMLServerHardware.cs. The wavemeter lock now is ready to fire.

## 3. Build, run and pray.

