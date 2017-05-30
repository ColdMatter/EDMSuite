using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DAQ.HAL;

namespace ICEBLOCTest
{
    [TestClass]
    public class DCSTests
    {
        [TestMethod]
        public void TestMessageSerialization()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            ICEBLOCCommunicator ice = new ICEBLOCCommunicator("192.168.1.237",1024);
            parameters["ip_address"] = "192.168.1.100";
            int[] transmission_id = {999};
            string op = "start_link";
            ICEBLOCMessage message = new ICEBLOCMessage(op, transmission_id, parameters);
            String json = ice.CreateJsonMessage(message);
            Console.WriteLine(json);
            ICEBLOCMessage message2 = ice.CreateICEBLOCMessage(json);
            Console.WriteLine(message2);
        }

        [TestMethod]
        public void TestDCSLink()
        {
            DCSController dcs = new DCSController("192.168.1.237", 1024);
            dcs.StartLink();
            dcs.Disable();
        }

        [TestMethod]
        public void TestPLLLink()
        {
            PLLController pll = new PLLController("192.168.1.228", 1024);
            pll.StartLink();
            pll.Disable();
        }

        [TestMethod]
        public void TestPLLFrequency()
        {
            PLLController pll = new PLLController("192.168.1.228", 1024);
            pll.StartLink();
            pll.UnlockMainLock();
            pll.SetBeatFrequency(6.834862000);
            pll.LockMainLock();
            Console.WriteLine("Set PLL frequency to 6.834682 GHz");
            pll.UnlockMainLock();
            pll.SetBeatFrequency(6.834864000);
            pll.LockMainLock();
            Console.WriteLine("Set PLL frequency to 6.834882 GHz");
            pll.UnlockMainLock();
            pll.SetBeatFrequency(6.834884000);
            pll.LockMainLock();
            pll.Disable();

        }

        
    }
}
