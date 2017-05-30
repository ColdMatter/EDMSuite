using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DAQ.HAL;
using NationalInstruments.VisaNS;

namespace ICEBLOCTest
{
    class ICEBloc
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting to ICE-BLOC PLL...");
            PLLController pll;
            try
            {
               pll = new PLLController("192.168.1.228", 1024);
               pll.StartLink();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to PLL:" + e.Message);
                return;
            }
            Console.WriteLine("Connected successfully!");
            Console.WriteLine("Wait for Values from Serial Port? [y/n]:");
            if (Console.ReadLine() == "y")
            {
               SerialSession comm = new SerialSession("ASRL22::INSTR");

               Console.WriteLine("Listening on COM22");
               while (true)
               {
                   try
                   {
                       String response = comm.ReadString();
                       Console.WriteLine("Setting PLL to " + response);
                       double ddsValue = Double.Parse(response);
                       pll.UnlockMainLock();
                       pll.SetBeatFrequency(ddsValue);
                       pll.LockMainLock();
                   }
                   catch
                   {

                   }
                  
               }
 
            }
            else
            {
                Console.WriteLine("Enter DDS Frequency in GHz:");
                double ddsValue = Double.Parse(Console.ReadLine());
                try
                {
                    pll.SetBeatFrequency(ddsValue);
                }
                catch
                {
                    Console.WriteLine("Could not set DDS Frequency");
                }

            }
            pll.Disable();
            

        }
    }
}
