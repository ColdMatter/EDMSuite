using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DecelerationConfig
{
    /*
     * Reads a file that gives the Stark shift of the rigid rotor, and provides a method to return the Stark
     * shift for a given state and electric field value
     */
    public static class RotorStark
    {
        //This is the data file. Its path is computer-specific
        private static string filename;

        //My file has the data for all (J,M) states up to and including J=5, 21 states in total
        const int NUMBER_OF_STATES = 21;

        //The file has all values of epsilon from 0 to 200. 
        //Between epsilon=0 and 1, the resolution of the points is 0.001.
        //Betweem epsilon=1 and 10, the resolution of the points is 0.01.
        //Between espilon=10 and 100, the resolution of the points is 0.1.
        const int POINTS_PER_STATE = 3801;
        const double RESOLUTION1 = 0.001;
        const double RESOLUTION2 = 0.01;
        const double RESOLUTION3 = 0.1;

        private static double[,] stark;

        static RotorStark()
        {
            //seems a bit clumsy...
            String computerName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
            switch (computerName)
            {
                case "SCHNAPS":
                    filename = "d:\\Tools\\starkTable.txt";
                    break;
                case "GANYMEDE0":
                    filename = "d:\\Experiment Control\\Packages\\starkTable.txt";
                    break;
                default:
                    filename = "d:\\Mike\\Work\\Manipulating molecules\\Decelerate\\starkTable.txt";
                    break;
            }
            
            // make an array to hold the Stark shift data
            stark = new double[NUMBER_OF_STATES, POINTS_PER_STATE];

            // open the stream
            StreamReader str = new StreamReader(filename);
            String line;
            string[] valsString;

            for (int j = 0; j < NUMBER_OF_STATES; j++)
            {
                line = str.ReadLine(); // read the line - it contains all the values for one state, separated by commas
                valsString = line.Split(','); //split it into its elements
                for (int i = 0; i < POINTS_PER_STATE; i++)
                {
                    stark[j, i] = Double.Parse(valsString[i]); //convert to doubles
                }
            }
            str.Close();
        }

        /*
         * Returns the rigid rotor Stark shift for the state (j,m), at the normalized field value epsilon.
         * The normalized field epsilon is (electric field) / (B / mu).
         */
        public static double StarkShift(int j, int m, double epsilon)
        {
            int low;
            double delta;
            // we have to take care of the way the data is split up into three blocks with three different resolutions
            if (epsilon <= 1)
            {
                low = (int)Math.Floor(epsilon / RESOLUTION1);
                delta = (epsilon / RESOLUTION1) - low;
            }
            else if (epsilon > 1 && epsilon <= 10)
            {
                low = 1000 + (int)Math.Floor((epsilon - 1) / RESOLUTION2);
                delta = 1000 + ((epsilon - 1) / RESOLUTION2) - low;
            }
            else
            {
                low = 1900 + (int)Math.Floor((epsilon - 10) / RESOLUTION3);
                delta = 1900 + ((epsilon - 10) / RESOLUTION3) - low;
            }

            double starkLow = stark[StateIndex(j, m), low];
            double starkHigh = stark[StateIndex(j, m), low + 1];
            return ((1 - delta) * starkLow) + (delta * starkHigh);  //linear interpolation
        }

        // The states are stored in the order (0,0),(1,0),(1,1),(2,0),(2,1),(2,2),(3,0)...
        private static int StateIndex(int j, int m)
        {
            return m + ((j * j) + j) / 2;
        }
    }
}
