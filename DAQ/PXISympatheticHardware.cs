using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the edm machine has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class PXISympatheticHardware : DAQ.HAL.Hardware
    {

        public PXISympatheticHardware()
        {

            // add the boards
            Boards.Add("multiDAQ", "/PXI1Slot6");
            Boards.Add("aoBoard", "/PXI1Slot5");
            Boards.Add("usbDAQ", "/Dev1");
            

            string multiDAQ = (string)Boards["multiDAQ"];
            string aoBoard = (string)Boards["aoBoard"];
            string usbDAQ = (string)Boards["usbDAQ"];
            
            // add things to the info
            Info.Add("PGClockLine", multiDAQ + "/PFI14");
            Info.Add("PatternGeneratorBoard", multiDAQ);
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("APGClockCounter", aoBoard + "/ctr0");
            Info.Add("AOPatternTrigger", aoBoard + "/PFI0");

            Info.Add("Element", "Li");
            //Test this
            //Info.Add("PGType", "dedicated");
            Info.Add("PGType", "integrated");

           
            // map the digital output channels
            // Control of atoms
            AddDigitalOutputChannel("MOTMasterPatternTrigger", multiDAQ, 0, 0);
            
            AddDigitalOutputChannel("aom0enable", multiDAQ, 0, 0);
            AddDigitalOutputChannel("aom1enable", multiDAQ, 0, 1);
            AddDigitalOutputChannel("aom2enable", multiDAQ, 0, 2);
            AddDigitalOutputChannel("aom3enable", multiDAQ, 0, 3);

            AddDigitalOutputChannel("CameraTrigger", multiDAQ, 0, 4);
            AddDigitalOutputChannel("AnalogPatternTrigger", multiDAQ, 0, 5);

            
            /*
            //Control of molecules
            AddDigitalOutputChannel("valve", multiDAQ, 0, 0); 
            AddDigitalOutputChannel("valve2", multiDAQ, 0, 1);
            AddDigitalOutputChannel("q", multiDAQ, 0, 2);
            AddDigitalOutputChannel("discharge", multiDAQ, 0, 3);
            AddDigitalOutputChannel("aom", multiDAQ, 0, 4); 
            AddDigitalOutputChannel("flash2", multiDAQ, 0, 5); 
            AddDigitalOutputChannel("q2", multiDAQ, 0, 6); 
            AddDigitalOutputChannel("detector", multiDAQ, 0, 7);
            AddDigitalOutputChannel("detectorprime", multiDAQ, 0, 8);
            AddDigitalOutputChannel("flash", multiDAQ, 0, 9);
             */
            
            

            // map the analog input channels
            AddAnalogInputChannel("pmt", multiDAQ + "/ai0", AITerminalConfiguration.Rse); //Pin 68
            AddAnalogInputChannel("lockcavity", multiDAQ + "/ai1", AITerminalConfiguration.Rse); //Pin 33
            AddAnalogInputChannel("probepower", multiDAQ + "/ai9", AITerminalConfiguration.Rse); //Pin 66

            AddAnalogInputChannel("laserLockErrorSignal", multiDAQ + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("chamber1Pressure", usbDAQ + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("chamber2Pressure", usbDAQ + "/ai1", AITerminalConfiguration.Differential);
            // map the analog output channels
            // Control of atoms
            AddAnalogOutputChannel("aom0amplitude", aoBoard + "/ao16");
            AddAnalogOutputChannel("aom0frequency", aoBoard + "/ao9");
            AddAnalogOutputChannel("aom1amplitude", aoBoard + "/ao10");
            AddAnalogOutputChannel("aom1frequency", aoBoard + "/ao11");
            AddAnalogOutputChannel("aom2amplitude", aoBoard + "/ao12");
            AddAnalogOutputChannel("aom2frequency", aoBoard + "/ao13");
            AddAnalogOutputChannel("aom3amplitude", aoBoard + "/ao14");
            AddAnalogOutputChannel("aom3frequency", aoBoard + "/ao15");
            AddAnalogOutputChannel("coil0current", aoBoard + "/ao8");
            AddAnalogOutputChannel("coil1current", aoBoard + "/ao17");
            AddAnalogOutputChannel("laser", aoBoard + "/ao1");
            AddAnalogOutputChannel("cavity", multiDAQ + "/ao1");


            //Control of molecules
            //AddAnalogOutputChannel("laser", aoBoard + "/ao0"); // Pin 22
            //AddAnalogOutputChannel("highvoltage", aoBoard + "/ao1"); // Note - this is just here because a channel called "highvoltage" has been hard-wired into DecelerationHardwareControl - this needs to be rectified
            //AddAnalogOutputChannel("cavity", aoBoard + "/ao1"); // Pin 21

            // map the counter channels
            AddCounterChannel("pmt", multiDAQ + "/ctr0"); //Source is pin 37, gate is pin 3, out is pin 2
            AddCounterChannel("sample clock", multiDAQ + "/ctr1"); //Source is pin 42, gate is pin 41, out is pin 40

            // Calibrations
            AddCalibration("chamber1Pressure", new PowerCalibration(1, 0, 10.875, 1, 10));
            AddCalibration("chamber2Pressure", new PowerCalibration(1, 0, 10.875, 1, 10));
            //AddCalibration("aom3frequency", new PolynomialCalibration
                //(new double[] {-27.2757, 0.698297, -0.0075598, 0.000045057, -1.33872 * Math.Pow(10,-7), 1.57402* Math.Pow(10, -10)}));
            AddCalibration("aom0frequency", new PolynomialCalibration(new double[] 
            {9.73471, -0.389447, 0.00439124, -0.0000200009, 4.27697*Math.Pow(10,-8), -3.44365*Math.Pow(10,-11)}, 130, 260));
            AddCalibration("aom1frequency", new PolynomialCalibration(new double[] 
            {-11.9562, 0.185676, -0.00161757, 0.0000109047, -3.54351*Math.Pow(10,-8), 4.35218*Math.Pow(10,-11)}, 130, 260));
            AddCalibration("aom2frequency", new PolynomialCalibration(new double[] 
            { 0.471968, -0.139565, 0.00173958, -6.18839 * Math.Pow(10, -6), 7.4987 * Math.Pow(10, -9), 8.99272 * Math.Pow(10, -13) }, 130,260));
            AddCalibration("aom3frequency", new PolynomialCalibration(new double[] 
            {0.879515, -0.143097, 0.00170292, -5.6672*Math.Pow(10,-6), 5.44491*Math.Pow(10,-9), 3.56736*Math.Pow(10,-12)},130,260));
            //AddCalibration("aom3amplitude", new ExponentialAndPolynomialCalibration(1, 2, 3, new double[] {}));
            AddCalibration("coil0current", new LinearInterpolationCalibration(new double[,] {{0.0, 0.0}, {0.0, 0.5}, {0.23, 0.75}, {0.82, 1}, {1.44, 1.25}, 
            {2.1, 1.5}, {2.75, 1.75}, {3.41, 2}, {4.73, 2.5}, {6.08, 3}, {7.4, 3.5}, {8.76, 4}, {10.08, 4.5}, {11.45, 5}, {12.77, 5.5}, {14.14, 6}, 
            {15.46, 6.5}, {16.83, 7}, {17.48, 7.25}, {18.15, 7.5}, {18.83, 7.75}, {19.53, 8}, {19.98, 8.5}, {19.98, 9}, {19.98, 9.5}, {19.98, 10}}));
        }

    }
}