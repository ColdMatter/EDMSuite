using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DecelerationConfig
{
    public class Decelerator
    {

        private FieldMap map;
        private double voltage; //in kV
        private double lensSpacing; //in m;

        public Decelerator()
        {           
        }

        public double Voltage
        {
            get { return voltage; }
            set { voltage = value; }
        }

        public FieldMap Map
        {
            get { return map; }
            set { map = value; }
        }

        public double LensSpacing
        {
            get { return lensSpacing; }
            set { lensSpacing = value; }
        }

        // the field map in V/m
        public double[] Field
        {
            get 
            {
                double[] theMap = map.Map;
                double[] temp = new double[theMap.Length];
                for (int i = 0; i < temp.Length; i++) temp[i] = 1000 * voltage * theMap[i];

                return temp;
            }
        }

        public double GetField(double position)
        {
            return 1000 * voltage * map.GetFieldAt(position);
        }

    }
}
