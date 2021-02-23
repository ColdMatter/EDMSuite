using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;


namespace CaFCon
{
    public static class Hardware
    {
        //private static AnalogSingleChannelReader therm4KRTReader;

        //static Hardware()
        //{
        //   //therm4KRTReader = CreateAnalogInputReader("4KRTthermistor");
        //}

        [DllExport("iexist", CallingConvention = CallingConvention.Cdecl)]
        public static bool iexist()
        {
            return true;
        }

        //[DllExport("get_temp", CallingConvention = CallingConvention.Cdecl)]
        //private static double GetTemperature()
        //{
        //    double therm4KRTVoltage = therm4KRTReader.ReadSingleSample();
        //    return therm4KRTVoltage;
        //}

        //private static AnalogSingleChannelReader CreateAnalogInputReader(string channelName)
        //{
        //    Task task = new Task();
        //    ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(task, -10.0, 10.0);
        //    task.Control(TaskAction.Verify);
        //    return new AnalogSingleChannelReader(task.Stream);
        //}
    }
}
