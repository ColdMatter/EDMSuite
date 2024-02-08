## About The Project

Wavemeter viewer is designed to look up the wavemeter readings on another computer via TCP/IP, instead of the caveman method of using a VNC viewer.

### Installation

1. In DAQ/EnvironHelper.cs, under the config of your computer, add the name of the computer that runs the wavemeter, and the TCP channel that the controller is on.
   ```sh
   case "YourComputerName":
     Hardware = new YourComputerHardware();
     FileSystem = new YourComputerFileSystem();
     viewerServerComputerName = "ServerComputerName";
     viewerServerTCPChannel = 0000;
     break;
   ```

2. Done
