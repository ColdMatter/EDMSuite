using System;
using System.Collections;
using System.IO;

using DAQ.Environment;
using Data;

namespace DAQ.FakeData
{
	/// <summary>
	/// 
	/// </summary>
	public class DataFaker
	{
		static DataFaker()
		{
			String path = (String)Environs.FileSystem.Paths["fakeData"];
			foreach (String file in Directory.GetFiles(path,"*.zip"))
			{
				String[] parts = file.Split('\\');
				String name = parts[parts.Length - 1].TrimEnd(".zip".ToCharArray());
				FakeScans.Add(name, file);
			}
		}

		public static Hashtable FakeScans = new Hashtable();

		public static Shot GetFakeShot(int gateStart, int gateLength, int clockPeriod, double intensity,
										int numberOfDetectors)
		{
			Random rng = new Random();
			// generate some fake data
			double[] detectorOnData = new double[gateLength];
			double newRand = rng.NextDouble();
			int centre = gateLength/2;
			for (int i = 0; i < gateLength; i++) 
			{
				detectorOnData[i] = (5 * rng.NextDouble()) + 5 * intensity *
					Math.Exp(-Math.Pow((i - centre),2)/(0.9 * gateLength));
			}

			TOF tofOn = new TOF();
			tofOn.Data = detectorOnData;
			tofOn.GateStartTime = gateStart;
			tofOn.ClockPeriod = clockPeriod;
			tofOn.Calibration = 1;
			Shot sOn = new Shot();
			for (int j = 0 ; j < numberOfDetectors ; j++) sOn.TOFs.Add(tofOn);
			return sOn;
		}

		public static String GetFakeDataPath(String key)
		{
			return (String)FakeScans[key];
		}

	}
}
