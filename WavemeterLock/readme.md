<a name="readme-top"></a>
<!-- ABOUT THE PROJECT -->
## About The Project

Wavemeter Lock is a simple programme to lock your laser using Highfinesse wavemeter. The server programme access the wavemeter reading via wlmData.dll and publish its controller on the local network via TCP/IP protocal. The clients get the reading remotely and send feedback voltage to the slave lasers.

Wavemeter Lock features foolproof set-up and operation, minimal lab space occupation, fast lock rate (>10Hz), deep lock range (hundreds of nm), high precesion and no long term drift (if calibrated once or twice a day). 

<p align="right">(<a href="#readme-top">back to top</a>)</p>




<!-- GETTING STARTED -->
## Getting Started



### Prerequisites

Here are the things you need to have before installing Wavemeter Lock.

* A Highfinesse wavemeter with multi-channel fiber switch connected to the server computer
* Highfinesse wavemeter software installed and running on the server computer
* An National Instrument DAQ board installed on the client computer(s) with at leaset one analog output channel available
* A voltage controllable single mode laser, hooked up to said analog output channel
* A single mode optical fiber, transporting some laser power to the wavemeter multi-channel fiber switch (~0.5mW should be sufficient)
  

### Installation

1. In DAQ/EnvironHelper.cs, under the config of the server computer, add the TCP channel number for the Wavemeter Lock Server to hand over its controller.
   ```sh
   case "ServerComputerName":
      Hardware = new ServerComputerHardware();
      FileSystem = new ServerComputerFileSystem();
      serverTCPChannel = 0000;
      break;
   ```
   Of course replace 0000 with your favourite four digit integer. The wavemeterlock server setup is done.

2. In DAQ/EnvironHelper.cs, under the config of the client computer, add the name of the server computer for this client, add the same TCP channel number, then add another TCP channel for Wavemeter Lock to hand over its controller.
   ```sh
   case "ClientComputerName":
      Hardware = new ClientComputerHardware();
      FileSystem = new ClientComputerFileSystem();
      serverComputerName = "ServerComputerName";
      serverTCPChannel = 0000;
      wavemeterLockTCPChannel = 1111;
      break;
   ```

3. In the hardware config class of client computer DAQ/ClientHardware.cs, create the Wavemeter Lock configuration.
   ```sh
   WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
   ```

4. In the hardware config class of client computer DAQ/ClientHardware.cs, config the analog output to the laser(s).
   ```sh
   wmlConfig.AddSlaveLaser("YourLaserName", "YourAnalogChannel", 0);
   ```
   Replace 0 with the wavemeter channel number of your laser.
   
5. To add lock block function, in DAQ/ClientHardware.cs
   ```sh
   wmlConfig.AddLockBlock("YourLaserName", "YourDigitalChannel");
   ```
   Then YourLaserName lock will be blocked if you send a high digital signal to YourDigitalChannel.
   
6. To config the initial setpoints and gains, in DAQ/ClientHardware.cs, add
   ```sh
   wmlConfig.AddLaserConfiguration("YourLaserName", SetFrequencyInTHz, PGain, IGain);
   ```

7. In the hardware config class of client computer DAQ/ClientHardware.cs, add the configuration you just created.
   ```sh
   Info.Add("Default", wmlConfig);
   ```

8. In the file system class of client computer DAQ/ClientFileSystem.cs, add directory for logging the data.
   ```sh
   Paths.Add("wavemeterLockData", "YourDirectory\\");
   ```
   The laser data and setpoints will be saved here. Click Save SetPoints to save current setpoints and gains, click Load SetPoints to load the most recent settings.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Frequently occured issues

* License issue: 

  Delete WavemeterLock/Properties/licenses.licx. 
  In Visual Studio, click Extension drop down menu, select Measurement Studio, refresh license.
  
* Wavemeter lock stops updating:

  Make sure the switch mode is on, and channel 1 is reading normally. The server only triggers an event when channel 1 gets a new reading for some reason.

## Operation

1. Run the Highfinesse wavemter programme on the server computer, turn on switch mode.
2. Run the Wavemeter Lock Server on the server computer, make sure channel 1 is operating normally.
3. Run the Wavemeter Lock on the client computer.
4. Go crazy.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## EDMSuite Compatibility

As part of the EDMSuite, Wavemeter Lock should communicate with other projects seamlessly.

Currently constructed plugins:
* ScanMaster, read frequency and scan setpoint
* MOTMaster via python, scan setpoint


## Current issues
* Remote event subscribtion from client to server in WavemeterLock/Controller.cs
   ```sh
   wavemeterContrller.measurementAcquired += () => { updateLockMaster(); };
   ```
   raises a system.security exception. It was circumvented by creating a shared dictionary between clients and server, but it will be nice to make it work in the future. 

* The callback process was established in WavemeterLockServer/Controller.cs as follows:
   ```sh
   callbackObj = new WLM.CallbackProcEx(callback);
   WLM.Instantiate(WLM.cInstNotification, WLM.cNotifyInstallCallback, callbackObj, 0);

   private void callback(int Ver, int Mode, int IntVal, double DblVal, int Res1)
      {
         switch (Mode)
            {
               case WLM.cmiResultMode:
                  measurementAcquired?.Invoke();
                  break;
            }
         }
   ```
 The measurement acquired event was raised only when channel 1 gets a new reading. And the build-in callback process in general didn't work as they stated in the manual, or I made some mistakes.

## Update Roadmap
The goal for future updates:
- Add autocalibration function
- Add lock update rate on the server pannel


<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Update Log

* [v 1.2.0] November 11th 2023: Fixed the lock block issue by putting read block flag channel on another thread. Added log data function for individual lasers. Added save and load setpoints function.
* [v 1.1.2] August 25th 2023: Added RMS noise pannel.
* [v 1.1.1] August 23rd 2023: Added an array of LEDs at front pannel to indicate laser lock status, added lock all button so you can click less. Removed the pop up window saying "you messed up" when the laser unlocks.
* [v 1.1.0] May 30st 2023: Made compatable with more than one servers running at the same time.
* [v 1.0.5] May 26th 2023: Added an error message upon initial connection failure.
* [v 1.0.4] April 18th 2023: Changed lock block digital channel reading method from event triggered to polling, increased stability and made compatible with PFI channels.
* [v 1.0.3] April 7th 2023: Added laser initial configuration. You can now set the default set frequency and gains. It is useful if you have multiple lasers with known optimized setpoints and gains.
* [v 1.0.2] April 6th 2023: Added lock block function. Wavemeter lock can now be blocked via an external TTL signal, you can use it to temporarily chirp or modulate your laser.
* [v 1.0.1] April 5th 2023: Added a list in server showing connected clients
* [v 1.0.0] April 1st 2023: Changed Wavemeter Lock update method from polling to measurement triggered, updated readme.md


<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact
Please contact Qinshu Lyu Github/lyuqinshu if you find any issues or have any suggestions.

<!-- ACKNOWLEDGMENT -->
## Acknowledgment
I thank Arijit Chakraborty and Lali Palanki for their contributions to WavemeterLock. 

Ain't nothing's gonna work without 'em.



<p align="right">(<a href="#readme-top">back to top</a>)</p>






