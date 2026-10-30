using System;
using System.Collections;
using System.Collections.Generic;
using DAQ.Analog;
using DAQ.HAL;
using DAQ.Pattern;
using MOTMaster;

namespace MOTMasterTests
{
    public static class LastRunOutputsTests
    {
        public static void TestAnalogueEndValueIsTheLatestEvent()
        {
            var analog = new AnalogPatternBuilder(200);
            analog.AddBoard("/A1");
            AnalogPatternBuilderSingleBoard board = analog.Boards["/A1"];
            board.AddChannel("aA");
            board.AddAnalogValue("aA", 0, 1.0);
            board.AddAnalogValue("aA", 100, 3.0);
            board.AddAnalogValue("aA", 50, 2.0);
            board.AddChannel("aCalibrated");
            board.AddAnalogValue("aCalibrated", 0, 1.0);
            var calibrations = new Hashtable { { "aCalibrated", new LinearCalibration(2.0, 0.0, 0.0) } };
            var sequence = new MOTMasterSequence { DigitalPattern = new PatternBuilder32(), AnalogPattern = analog };

            Dictionary<String, Object> values = LastRunOutputs.EndValues(sequence, new TestHardware().DigitalOutputChannels,
                calibrations);

            Assert.Equal<object>(3.0, values["aA"], "value at the latest event");
            Assert.True(!values.ContainsKey("aCalibrated"), "a calibrated channel is left out");
        }

        public static void TestDigitalEndValueIsTheLastSample()
        {
            TestHardware hardware = new TestHardware().Digital("high", "/D1", 0, 3).Digital("low", "/D1", 0, 4)
                .Digital("otherBoard", "/D2", 0, 3);
            var digital = new PatternBuilder32();
            digital.AddBoard("/D1");
            PatternBuilder32SingleBoard board = digital.Boards["/D1"];
            board.AddEdge(3, 10, true);
            board.AddEdge(4, 10, true);
            board.AddEdge(4, 20, false);
            digital.BuildPattern(50);
            var sequence = new MOTMasterSequence { DigitalPattern = digital, AnalogPattern = new AnalogPatternBuilder(50) };

            Dictionary<String, Object> values = LastRunOutputs.EndValues(sequence, hardware.DigitalOutputChannels,
                new Hashtable());

            Assert.Equal<object>(true, values["high"], "bit 3");
            Assert.Equal<object>(false, values["low"], "bit 4");
            Assert.True(!values.ContainsKey("otherBoard"), "a board the sequence does not use is left out");
        }

        public static void TestRecordDropsValuesWhenOutputsUnknown()
        {
            var finished = new DateTime(2026, 10, 29, 14, 2, 0, DateTimeKind.Utc);
            var values = new Dictionary<String, Object> { { "aA", 1.25 } };

            Dictionary<String, Object> record = LastRunOutputs.Record(7, "LoadTweezer", finished, false, values);

            Assert.Equal(5, record.Count, "key count");
            Assert.Equal<object>(7, record["RunNumber"], "RunNumber");
            Assert.Equal<object>("LoadTweezer", record["Script"], "Script");
            Assert.True(record["Finished"] is double && Math.Abs((double)record["Finished"] - 1793282520.0) < 1e-6,
                "Finished is Unix seconds, as a double");
            Assert.Equal<object>(false, record["OutputsKnown"], "OutputsKnown");
            var recorded = record["Values"] as Dictionary<String, Object>;
            Assert.True(recorded != null && recorded.Count == 0, "Values is an empty Dictionary<String, Object>");
        }
    }
}
