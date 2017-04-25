using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAQ.HAL;

namespace ICEBLOCTest
{
    [TestClass]
    public class DCSTests
    {
        [TestMethod]
        public void TestStartLink()
        {
            Console.WriteLine("Testing StartLink for DCS...");
            DCSController dcs = new DCSController("192.168.1.228", 39281);
            dcs.StartLink();
            Console.WriteLine("Successfully Linked to DCS.");
        }
    }
}
