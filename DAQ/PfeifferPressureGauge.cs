using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class PfeifferPressureGauge : DAQ.HAL.SerialInstrument
    {

        /// This document defines all the possible commands for the controller.
        /// </summary>

        public enum ErrorCode : ushort
        {
            None = 0,
            WatchdogResponded = 1,
            TaskFail = 2,
            IDCSIdle = 4,
            StackOverflow = 8,
            EPROM = 16,
            RAM = 32,
            EEPROM = 64,
            Key = 128,
            Syntax = 4096,
            InadmissibleParameter = 8192,
            NoHardware = 16384,
            FatalError = 32768
        }

        public enum SensorErrorCode : ushort
        {
            None = 0,
            Sensor1Measurement = 1,
            Sensor2Measurement = 2,
            Sensor3Measurement = 4,
            Sensor4Measurement = 8,
            Sensor5Measurement = 16,
            Sensor6Measurement = 32,
            Sensor1Id = 512,
            Sensor2Id = 1024,
            Sensor3Id = 2048,
            Sensor4Id = 4096,
            Sensor5Id = 8192,
            Sensor6Id = 16384
        }

        public enum PressureReadingStatus : ushort
        {
            MeasurementDataOK = 0,
            Underrange = 1,
            Overrange = 2,
            SensorError = 3,
            SensorOff = 4,
            NoSensor = 5,
            IDError = 6
        }

        // Serial connection parameters for the Pfeiffer MaxiGauge:
        protected new int BaudRate = 9600;
        protected new short DataBits = 8;
        protected new SerialStopBitsMode StopBit = SerialStopBitsMode.One;
        protected new SerialParity ParitySetting = SerialParity.None;
        protected new byte TerminationCharacter = 0x0A;
        protected new int TimeoutMilliseconds = 2000;


        public PfeifferPressureGauge(String address)
            :base(address)
        {
            base.BaudRate = 9600;
            base.DataBits = 8;
            base.ParitySetting = SerialParity.None;
            base.StopBit = SerialStopBitsMode.One;
            base.TerminationCharacter = 0x0A;
            base.TimeoutMilliseconds = 2000;
        }
        protected void Clear()
        {
            serial.Clear();
        }

        public double ReadPressure(int sensor)
        {
            byte[] readVal;
            string readStr;
            int status = 0;
            double pressure = 0.0;

            if (sensor < 1 || sensor > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(sensor), $"Sensor must be between 1 and 6.");
            }

            if (!connected)
            {
                Connect(SerialTerminationMethod.TerminationCharacter);
                connected = true;
            }
            serial.RawIO.Write(String.Concat("PR", Convert.ToString(sensor), "\r"));

            readVal = serial.RawIO.Read();
            if (readVal[0] == 0x06)
            {
                serial.RawIO.Write(new byte[] { 0x05, 0x0D }, 0, 2);
                readStr = serial.RawIO.ReadString();
                status = Convert.ToUInt16(readStr.Split(',')[0]);
                pressure = Convert.ToDouble(readStr.Split(',')[1]);
            }

            Disconnect();

            return pressure;

        }
    }
}
