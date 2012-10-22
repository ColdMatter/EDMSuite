using System;
using System.Collections;
using System.Collections.Generic;
using NationalInstruments.VisaNS;

using DAQ.Environment;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DAQ.HAL
{
    /// <summary>
    /// This class controlls the jaguar motor controller by passing commands to the BCD application. 
    /// To prevent the controller reseting to a neutral state, a hearbeat signal must be periodically sent by the application.
    /// This means that the application has to stay open, otherwise the controller will stop whatever it is doing (anoyingly), 
    /// which means that this instrument behaves  quite differently from other RS232 instruments. 
    /// </summary>
   
	public class SerialMotorControllerBCD : DAQ.HAL.Instrument
	{
        protected string address;
        protected bool connected = false;
        private static Process motorControllerSession = new Process();
        private StreamWriter inputStream; 
        private StreamReader outputStream;
        private double posDiffTwoPi = 327680;
        private double maxVoltSetting = 32767;
        private double minVoltSetting = 32768;

        public SerialMotorControllerBCD(String visaAddress)
        {
            int colonIndex = visaAddress.IndexOf(':');
            this.address = visaAddress.Remove(colonIndex, 7).Remove(0, 4);//Parses the com port number from the address string, assuming it is of the form "ASRL12::INSTR"
        }


        public override void Connect()
        {
            if (!Environs.Debug)
            {
                if (!Environs.Debug)
                {
                    motorControllerSession.StartInfo.FileName="bdc-comm-92.exe";
                    motorControllerSession.StartInfo.UseShellExecute = false;
                    motorControllerSession.StartInfo.RedirectStandardInput = true;
                    motorControllerSession.StartInfo.RedirectStandardOutput = true;
                    motorControllerSession.StartInfo.Arguments = "-c "+ address;
                    motorControllerSession.StartInfo.CreateNoWindow = true;

                    motorControllerSession.Start(); //Starts the motor controller application
                    Thread.Sleep(500);
                    inputStream = motorControllerSession.StandardInput;
                    Thread.Sleep(500);
                    outputStream = motorControllerSession.StandardOutput;
                    Thread.Sleep(500);
                }
                connected = true;
            }
        }

        public override void Disconnect()
        {
            if (!Environs.Debug)
            {
                inputStream.WriteLine("quit");
                inputStream.Close();
                motorControllerSession.Close();
            }
        }


        protected override void Write(string cmd)
        {
            if (!connected) Connect();
            outputStream.DiscardBufferedData();
            if (!Environs.Debug) inputStream.WriteLine(cmd);
            Thread.Sleep(500);
        }

        protected override string Read()
        {
            return outputStream.ReadLine();
        }
        
        public string ParsedRead()
        {
            string unParsedOutput = outputStream.ReadLine();
            int equalsIndex = unParsedOutput.IndexOf("= ");
            int spaceAfterNumIndex = unParsedOutput.IndexOf(" ", equalsIndex + 2);
            if (spaceAfterNumIndex != -1)
            {
                return unParsedOutput.Remove(spaceAfterNumIndex).Remove(0, equalsIndex + 2);
            }
            else
            {
                return unParsedOutput.Remove(0, equalsIndex + 2);
            }
        }

        //Changes angles into the position coordinates used by the controller
        public string AngleToPosition(double angle)
        {
            return Math.Round(posDiffTwoPi * (angle % 360) / 360).ToString();
        }

        public double PositionToAngle(string position)
        {
            return (360 * (double.Parse(position) % posDiffTwoPi))  / posDiffTwoPi;
        }

        //Setting to initialise the controller for our rotation mounts
        public void InitPolariserControl(string startPos)
        {
            Write("id 1"); //default board id
            //proportional and integral constants 
            Write("pos p 2147483647");
            Write("pos i 20000000");
            //expect quadrature encoder
            Write("pos ref 0");
            //number of lines on our encoder
            Write("config lines 12288");

            Write("pos en " + startPos);

        }

        public void PositionModeEnable(double startAngle)
        {
            string startingPos = AngleToPosition(startAngle);
            Write("pos en "+startingPos);
        }

        public void VoltageModeEnable()
        {
            Write("volt en");
            Write("volt set 0");
        }

        public void InitPolariserControl()
        {
            Write("id 1"); //default board id
            //proportional and integral constants 
            Write("pos p 2147483647");
            Write("pos i 20000000");
            //expect quadrature encoder
            Write("pos ref 0");
            //number of lines on our encoder
            Write("config lines 12288");

            Write("pos en 0");

        }

        public void SetPosition(double Angle)
        {
            string angleAsString = AngleToPosition(Angle);
            Write("pos set " + angleAsString);
        }

        public void SetPosition(string position)
        {
            Write("pos set " + position);
        }

        public double MeasurePosition()
        {
            
            Write("stat pos");
            string position = ParsedRead();
            return PositionToAngle(position);
        }

        //public string MeasureVoltage()
        //{

        //    Write("stat vout");
        //    double voltageOut= Double.Parse(ParsedRead());
        //    double voltageToSend;

        //    if (voltageOut >= 0)
        //    {
        //        voltageToSend = voltageOut/ maxVoltSetting;
        //    }
        //    else
        //    {
        //        voltageToSend = voltageOut / minVoltSetting;
        //    }

        //    return
        //}

        public void SetMotorVoltage(double fractionOfMaxVoltage)
        {
            int voltageToSend;
            if (fractionOfMaxVoltage >= 0)
            {
                voltageToSend = (int)Math.Round(fractionOfMaxVoltage * maxVoltSetting);
            }
            else
            {
                voltageToSend = (int)Math.Round(fractionOfMaxVoltage * minVoltSetting);
            }
            Write("volt set " + voltageToSend);
        }


        public void SetRandomPosition()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, (int)posDiffTwoPi);
            SetPosition(randomNumber.ToString());
        }

        public void ReturnToZero()
        {
            SetPosition(0);
        }

    }
}
