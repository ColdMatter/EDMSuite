using System;
using System.Collections.Generic;

namespace MOTMaster.SnippetLibrary.CaF
{
    // The four DDS channels' frequencies (MHz) and amplitudes for each stage, read from the
    // parameters, in the order DDSPatternBuilder.AddEvent takes them (DDS1 to DDS4).
    public static class CaFDDS
    {
        // Parameters name + "1" to name + "4".
        public static double[] FourChannels(Dictionary<String, Object> parameters, string name)
        {
            return new double[] {
                (double)parameters[name + "1"], (double)parameters[name + "2"],
                (double)parameters[name + "3"], (double)parameters[name + "4"] };
        }

        public static double[] MOTFrequencies(Dictionary<String, Object> parameters)
        {
            return FourChannels(parameters, "MOTFreqDDS");
        }

        public static double[] MOTAmplitudes(Dictionary<String, Object> parameters)
        {
            return FourChannels(parameters, "MOTAmpDDS");
        }

        // Lambda cooling: the F=1- and F=1+ tones on DDS1 and DDS4, DDS2 and DDS3 off.
        public static double[] LambdaFrequencies(Dictionary<String, Object> parameters)
        {
            return new double[] {
                (double)parameters["LambdaF1minus"], (double)parameters["MOTFreqDDS2"],
                (double)parameters["MOTFreqDDS3"], (double)parameters["LambdaF1plus"] };
        }

        public static double[] LambdaAmplitudes(Dictionary<String, Object> parameters)
        {
            return new double[] {
                (double)parameters["Lambda1Amp"], (double)parameters["LightoffDDS2"],
                (double)parameters["LightoffDDS3"], (double)parameters["Lambda2Amp"] };
        }

        public static double[] BlueMOTFrequencies(Dictionary<String, Object> parameters)
        {
            return FourChannels(parameters, "FreqCVB");
        }

        public static double[] BlueMOTAmplitudes(Dictionary<String, Object> parameters)
        {
            return FourChannels(parameters, "BMOTAmpDDS");
        }

        public static double[] ResonanceFrequencies(Dictionary<String, Object> parameters)
        {
            return FourChannels(parameters, "ResonanceDDS");
        }

        // The same amplitude on all four channels, e.g. LightoffDDS1 to switch the light off.
        public static double[] Same(Dictionary<String, Object> parameters, string name)
        {
            double a = (double)parameters[name];
            return new double[] { a, a, a, a };
        }
    }
}
