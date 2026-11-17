using System;
using System.Collections.Generic;
using DAQ.Analog;
using DAQ.Pattern;
using MOTMaster;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    public static class ScriptBaseTests
    {
        // Mirrors how real scripts build Parameters: load shared values first, then derive from them.
        class DerivedScript : MOTMasterScript
        {
            public DerivedScript()
            {
                Parameters = new Dictionary<string, object>();
                LoadGlobalParameters();
                Parameters["Derived"] = (int)Parameters["Base"] + 1;
            }
            public override PatternBuilder32 GetDigitalPattern() { return new PatternBuilder32(); }
            public override AnalogPatternBuilder GetAnalogPattern() { return new AnalogPatternBuilder(100); }
        }

        class FileScript : MOTMasterScript
        {
            public FileScript()
            {
                Parameters = new Dictionary<string, object>();
                LoadGlobalParameters();
            }
            public override PatternBuilder32 GetDigitalPattern() { return new PatternBuilder32(); }
            public override AnalogPatternBuilder GetAnalogPattern() { return new AnalogPatternBuilder(100); }
        }

        public static void TestDdsBuilderAvailable()
        {
            var b = new DDSPatternBuilder();
            b.AddEvent("e", 100, new double[4], new double[4]);
            Assert.Equal(1.0, b.Pattern["e"][0][0], "time in ms");
        }

        public static void TestLoadGlobalParametersUsesOverride()
        {
            try
            {
                MOTMasterScript.LinkedParameterOverride = new Dictionary<string, object>
                {
                    { "Base", 41 }, { "Extra", 2.5 }
                };
                var s = new DerivedScript();
                Assert.Equal(42, (int)s.Parameters["Derived"], "Derived");
                Assert.Equal(2.5, (double)s.Parameters["Extra"], "Extra");
                Assert.True(!s.Parameters.ContainsKey("BlueMOTDuration"), "no key from the real globalParameters.json");

                MOTMasterScript.LinkedParameterOverride = null;
                var real = new FileScript();
                Assert.True(real.Parameters.ContainsKey("BlueMOTDuration"), "real file read again once override cleared");
            }
            finally
            {
                MOTMasterScript.LinkedParameterOverride = null;
            }
        }
    }
}
