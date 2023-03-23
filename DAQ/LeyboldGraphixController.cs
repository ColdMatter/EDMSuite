using System;
using System.Collections.Generic;
using Ivi.Visa;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class LeyboldGraphixController : RS232Instrument
    {

        public static Dictionary<string, byte> SpecialCharacters = new Dictionary<string, byte>
        {
            { "EOT" , 0x04 },
            { "SI"  , 0x0f },
            { "SO"  , 0x0e },
            { "ACK" , 0x06 },
            { "NACK", 0x15 }
        };

        public struct Response
        {
            public bool ack;
            public string resp;
        
            public Response(bool _ack, string _resp)
            {
                ack = _ack;
                resp = _resp;
            }
        }

        public LeyboldGraphixController(string visaAddr) : base(visaAddr)
        {
            base.BaudRate = 38400;
            base.TerminationCharacter = SpecialCharacters["EOT"];
            base.DataBits = 8;
            base.StopBit = Ivi.Visa.SerialStopBitsMode.One;
            base.ParitySetting = Ivi.Visa.SerialParity.None;
            base.FlowControl = Ivi.Visa.SerialFlowControlModes.None;
        }

        private byte CalculateCRC(List<byte> s)
        {
            byte sum = 0;
            foreach (byte b in s)
            {
                sum += (byte)b;
            }
            byte crc = (byte)(0xFF - sum);
            if (crc < 0x20) return (byte) (crc + 0x20);
            return crc;
        }

        private Response Send(List<byte> s)
        {
            int attempt = 0;
        func_start:
            attempt++;
            if (attempt > 5) throw new Exception("More than 5 failed CRCs. Try restarting the controller.");
            List<byte> resp = new List<byte> { };
            if (!connected) Connect(SerialTerminationMethod.TerminationCharacter);
            if (!Environs.Debug)
            {
                s.Add(CalculateCRC(s));
                s.Add(SpecialCharacters["EOT"]);
                serial.RawIO.Write(s.ToArray());
                resp = new List<byte>(serial.RawIO.Read());
            }
                
            Disconnect();

            string value = System.Text.Encoding.ASCII.GetString(resp.ToArray()).Substring(1, resp.Count - 3);
            byte crc = resp[resp.Count - 2];
            byte ack = resp[0];
            List<byte> messageBody = new List<byte>(resp);
            messageBody.RemoveAt(messageBody.Count - 1);
            messageBody.RemoveAt(messageBody.Count - 1);
            byte trueCRC = CalculateCRC(messageBody);

            if (crc != trueCRC) goto func_start;
            if (ack == SpecialCharacters["ACK"])  return new Response(true,  value);
            if (ack == SpecialCharacters["NACK"]) return new Response(false, value);
            throw new Exception("Malformed response from device.");
        }

        private string RetryOnFailiureSend(List<byte> s)
        {
            for (int i = 0; i < 5; ++i)
            {
                Response resp = Send(s);
                if (resp.ack) return resp.resp;
            }
            throw new Exception("No valid response after 5 tries");
        }

        public string ReadValue(string s)
        {
            List<byte> command = new List<byte>(System.Text.Encoding.ASCII.GetBytes(s));
            command.Insert(0, SpecialCharacters["SI"]);
            return RetryOnFailiureSend(command);
        }

        public string WriteValue(string s)
        {
            List<byte> command = new List<byte>(System.Text.Encoding.ASCII.GetBytes(s));
            command.Insert(0, SpecialCharacters["SO"]);
            return RetryOnFailiureSend(command);
        }

        public string test()
        {
            List<byte> s = new List<byte> { 14, 49, 59, 53, 59, 118, 97, 99, 117, 117, 109, 32, 100, 4 };
            List<byte> resp = new List<byte> { };
            if (!connected) Connect(SerialTerminationMethod.TerminationCharacter);
            if (!Environs.Debug)
            {
                serial.RawIO.Write(s.ToArray());
                resp = new List<byte>(serial.RawIO.Read());
            }
            return System.Text.Encoding.ASCII.GetString(resp.ToArray());
        }

    }
}
