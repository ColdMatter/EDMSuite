using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class Eurotherm3504Instrument : RS232Instrument
    {

        private const ushort ProcessValue = 0x1;
        private const ushort SetPoint = 0x2;
        private const ushort ManualOutput = 0x3;
        private const ushort ActiveOutput = 0x4;
        private const ushort AMSwitch = 273;
        private byte InstrumentAddress;

        public struct Loop
        {
            public ushort ProcessValue;
            public ushort SetPoint;
            public ushort ManualOutput;
            public ushort AMSwitch;
            public ushort ActiveOutput;
            public ushort HeaterShutoff;

            public Loop(ushort ProcessValueAddr, ushort SetPointAddr, ushort ManualOutputAddr, ushort AMSwitchAddr, ushort ActiveOutAddr, ushort HeaterShutoffAddr)
            {
                ProcessValue = ProcessValueAddr;
                SetPoint = SetPointAddr;
                ManualOutput = ManualOutputAddr;
                AMSwitch = AMSwitchAddr;
                ActiveOutput = ActiveOutAddr;
                HeaterShutoff = HeaterShutoffAddr;
            }
        }


        private List<Loop> Loops = new List<Loop>();

        public Eurotherm3504Instrument(string visaAddr, byte address) : base(visaAddr)
        {
            base.BaudRate = 19200;
            base.StopBit = Ivi.Visa.SerialStopBitsMode.One;
            base.ParitySetting = Ivi.Visa.SerialParity.None;
            InstrumentAddress = address;
        }

        public void AddLoop(ushort ProcessValueAddr, ushort SetPointAddr, ushort ManualOutputAddr, ushort AMSwitchAddr, ushort ActiveOutAddr, ushort HeaterShutoffAddr)
        {
            Loops.Add(new Loop(ProcessValueAddr, SetPointAddr, ManualOutputAddr, AMSwitchAddr, ActiveOutAddr, HeaterShutoffAddr));
        }

        public void AddLoop(ushort offset, ushort HeaterShutoffAddr)
        {
            Loops.Add(new Loop((ushort)(ProcessValue + offset), (ushort)(SetPoint + offset), (ushort)(ManualOutput + offset), (ushort)(AMSwitch + offset), (ushort)(ActiveOutput + offset), HeaterShutoffAddr));
        }


        public void AppendCRC(List<byte> message)
        {
            ushort crc_reg = 0xFFFF;
            foreach (byte messageByte in message)
            {
                crc_reg ^= (ushort)(messageByte);
                for (int i = 0; i < 8; ++i)
                {
                    bool carryFlag = (crc_reg & 0x1) == 1;
                    crc_reg >>= 1;
                    if (carryFlag) crc_reg ^= 0xA001;
                }

            }
            message.Add((byte)(crc_reg & 0xFF));
            message.Add((byte)(crc_reg >> 8));
        }

        public List<byte> ReadResponse()
        {
            List<byte> response = new List<byte>();
            response.Add(serial.RawIO.Read(1)[0]);
            response.Add(serial.RawIO.Read(1)[0]);
            switch (response[response.Count() - 1])
            {
                case 0x06:
                    for (int i = 0; i < 6; ++i)
                    {
                        response.Add(serial.RawIO.Read(1)[0]);
                    }
                    return response;
                case 0x03:
                    response.Add(serial.RawIO.Read(1)[0]);
                    int readBytes = response[response.Count() - 1] + 2;
                    for (int i = 0; i < readBytes; ++i)
                    {
                        response.Add(serial.RawIO.Read(1)[0]);
                    }
                    return response;

            }
            return response;
        }

        public List<byte> WriteWord(ushort address, ushort val)
        {
            List<byte> res = new List<byte>();
            if (!connected) Connect();
            if (!Environs.Debug)
            {
                List<byte> message = new List<byte>();
                message.Add(InstrumentAddress);
                message.Add(0x6); // Write word
                message.Add((byte)(address >> 8));
                message.Add((byte)(address & 0xFF));
                message.Add((byte)(val >> 8));
                message.Add((byte)(val & 0xFF));
                AppendCRC(message);
                serial.RawIO.Write(message.ToArray());
                res = ReadResponse();
                if (res.Count() == 2) goto fail;
                List<byte> resCopy = new List<byte>();
                for (int i = 0; i < res.Count() - 2; ++i)
                {
                    resCopy.Add(res[i]);
                }
                AppendCRC(resCopy);
                if (res[res.Count() - 1] != resCopy[res.Count() - 1] || res[res.Count() - 2] != resCopy[res.Count() - 2]) goto fail;
            }
            else goto fail;

            res.RemoveAt(0);
            res.RemoveAt(0);
            res.RemoveAt(res.Count() - 1);
            res.RemoveAt(res.Count() - 1);
            Disconnect();
            return res;

        fail:
            Disconnect();
            return new List<byte>();

        }

        public List<byte> ReadWords(ushort address, ushort number)
        {
            List<byte> res = new List<byte>();
            if (!connected) Connect();
            if (!Environs.Debug)
            {
                List<byte> message = new List<byte>();
                message.Add(InstrumentAddress);
                message.Add(0x3); // Read words
                message.Add((byte)(address >> 8));
                message.Add((byte)(address & 0xFF));
                message.Add((byte)(number >> 8));
                message.Add((byte)(number & 0xFF));
                AppendCRC(message);
                serial.RawIO.Write(message.ToArray());
                res = ReadResponse();
                if (res.Count() == 2) goto fail;
                List<byte> resCopy = new List<byte>();
                for (int i = 0; i < res.Count() - 2; ++i)
                {
                    resCopy.Add(res[i]);
                }
                AppendCRC(resCopy);
                if (res[res.Count() - 1] != resCopy[res.Count() - 1] || res[res.Count() - 2] != resCopy[res.Count() - 2]) goto fail;
            }
            else goto fail;

            res.RemoveAt(0);
            res.RemoveAt(0);
            res.RemoveAt(0);
            res.RemoveAt(res.Count() - 1);
            res.RemoveAt(res.Count() - 1);
            Disconnect();
            return res;

        fail:
            Disconnect();
            return new List<byte>();
        }

        public double GetPV(int loop)
        {
            if (Environs.Debug) return double.NaN;
            List<byte> data = ReadWords(Loops[loop].ProcessValue, 1);
            short PV = 0;
            PV |= (short)(data[0] << 8);
            PV |= (short)(data[1] & 0xFF);
            return (double)PV / 100;
        }

        public void SetHeaterShutoff(int loop, bool Man)
        {
            if (Environs.Debug) return;
            WriteWord(Loops[loop].HeaterShutoff, Man ? (ushort)0x01 : (ushort)0x00);
        }

        public bool GetHeaterShutoff(int loop)
        {
            if (Environs.Debug) return false;
            List<byte> data = ReadWords(Loops[loop].HeaterShutoff, 1);
            return data[1] == 0x1;

        }

        public void SetAMSwitch(int loop, bool Man)
        {
            if (Environs.Debug) return;
            WriteWord(Loops[loop].AMSwitch, Man ? (ushort) 0x01 : (ushort) 0x00);
        }

        public bool GetAMSwitch(int loop)
        {
            if (Environs.Debug) return false;
            List<byte> data = ReadWords(Loops[loop].AMSwitch, 1);
            return data[1] == 0x1;

        }
        public void SetManOut(int loop, ushort thousanths)
        {
            if (Environs.Debug) return;
            WriteWord(Loops[loop].ManualOutput, thousanths);
        }

        public short GetManOut(int loop)
        {
            if (Environs.Debug) return 0x00;
            List<byte> data = ReadWords(Loops[loop].ManualOutput, 1);
            short ret = 0;
            ret |= (short)(data[0] << 8);
            ret |= (short)(data[1] & 0xFF);
            return ret;
        }

        public void SetSP(int loop, short SPinTenthDegreeC)
        {
            if (Environs.Debug) return;
            byte b = (byte) (SPinTenthDegreeC & 0xFF);
            ushort param = 0;
            param |= b;
            b = (byte)(SPinTenthDegreeC >> 8);
            param |= (ushort)(b << 8);
            WriteWord(Loops[loop].SetPoint, param);
        }
        public double GetSP(int loop)
        {
            if (Environs.Debug) return 0x00;
            List<byte> data = ReadWords(Loops[loop].SetPoint, 1);
            short ret = 0;
            ret |= (short)(data[0] << 8);
            ret |= (short)(data[1] & 0xFF);
            return (double)ret/10;
        }

        public double GetActiveOut(int loop)
        {
            if (Environs.Debug) return double.NaN;
            List<byte> data = ReadWords(Loops[loop].ActiveOutput, 1);
            short output = 0;
            output |= (short)(data[0] << 8);
            output |= (short)(data[1] & 0xFF);
            return (double)output / 10;
        }

    }
}
