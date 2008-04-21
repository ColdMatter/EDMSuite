using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DecelerationConfig
{
    public class FieldMap
    {
        private string filename;
        private int numberOfValues;
        private double startPoint;
        private double mapResolution;

        private double[] f;

        public FieldMap(string filename, int numberOfValues, double startPoint, double mapResolution)
        {
            this.filename = filename;
            this.numberOfValues = numberOfValues;
            this.startPoint = startPoint;
            this.mapResolution = mapResolution;

            StreamReader str = new StreamReader(filename);
            f = new double[numberOfValues];

            for (int i = 0; i < numberOfValues; i++)
            {
                f[i] = Double.Parse(str.ReadLine()); //one field value per line in the file
            }
            str.Close();
        }

        public double StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        public double MapResolution
        {
            get { return mapResolution; }
            set { mapResolution = value; }
        }

        public double[] Map
        {
            get { return f; }
        }

        public double GetFieldAt(double position)
        {
            int low = (int)Math.Floor((position - startPoint) / mapResolution);
            double fLow = f[low];
            double fHigh = f[low + 1];

            double delta = ((position - startPoint) / mapResolution) - low;
            return ((1 - delta) * fLow) + (delta * fHigh);  //linear interpolation
        }
    }
}
