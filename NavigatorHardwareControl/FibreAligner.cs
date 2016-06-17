using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;
using System.IO;
using System.Diagnostics;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// This class is used to align the retro-reflecting mirror. Using back-coupled laser power into the fibre, it aligns the mirror with respect to the fibre by maximising the measured photodiode power.
    /// </summary>
    public class FibreAligner
    {
        private Double[,] scanData;
        private AOChannel horizPiezo;
        private AOChannel vertPiezo;
        private AIChannel refInput;

        private List<fibrePower> fibreCoords;
        /// <summary>
        /// Initialises the aligner using channels defined in the hardware controller
        /// </summary>
        /// <param name="horizPiezo">Channel used to output the voltage to the Horizontal piezo</param>
        /// <param name="vertPiezo">Channel used to output the voltage to the Vertical piezo</param>
        /// <param name="refInput">Channel used for measuring the reflected power coupled back into the fibre</param>
        public FibreAligner(AOChannel horizPiezo, AOChannel vertPiezo, AIChannel refInput)
        {
            this.horizPiezo = horizPiezo;
            this.vertPiezo = vertPiezo;
            this.refInput = refInput;
        }
        /// <summary>
        /// Initialises the aligner without defining the channels. Typically only used for testing purposes.
        /// </summary>
        public FibreAligner()
        {
            this.horizPiezo = null;
            this.vertPiezo = null;
            this.refInput = null;
        }
        public struct fibrePower
        {
            //This is used to encapsulate the parameters for the fibre power - coordinates if using a testMap and voltages if aligning
            public double power;
            public int[] coords;
            public double[] voltages;

            public fibrePower(int[] coords, double power)
            {
                this.coords = coords;
                this.power=power;
                this.voltages = new double[2];
            }

            public fibrePower(double[] voltage, double power)
            {
                this.voltages = voltage;
                this.power = power;
                this.coords = new int[2];
            }
        }
        /// <summary>
        /// Using some test data, this executes the Nelder-Mead simplex algorithm to find the maximum value
        /// </summary>
        /// <param name="scanData"></param>
        /// <returns>Coordinates of the maximum value</returns>
        public int[] TestScan(double threshold)
        {
            //these are default values which could be modified if necessary
            double refScale = 1.0;
            double expScale = 2.0;
            double contScale = 0.5;
            double shrinkScale=0.5;
            Random r = new Random();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //this is a list of accepted points. used to check for convergence at the ordering step.
            fibreCoords = new List<fibrePower>();
            int maxX = scanData.GetLength(0);
            int maxY = scanData.GetLength(1);
            int[] outputCoords = new int[2];
            double outputPower;

            //select three random coordinates and gets the power from the scanData
            List<fibrePower> simplexCoords = new List<fibrePower>();
            for (int i = 0; i<3; i++)
            {
                int[] coords = randomCoords(maxX,maxY,r);
                fibrePower fibre = new fibrePower(coords, 0.0);
                fibre.power = getPowerFromCoords(fibre, scanData);
                simplexCoords.Add(fibre);
                fibreCoords.Add(fibre);
             }
        Order:
            simplexCoords = simplexCoords.OrderByDescending(d => d.power).ToList();
            if (fibreCoords.Count > 5)
            {
                //finds the standard deviation of the power of the final 5 points and uses a specified threshold to check if they have sufficiently converged
                //double stdDev = CalculateStdDev(fibreCoords.GetRange(fibreCoords.Count - 5, 5).Select(d => d.power));
                if (simplexCoords[0].power > threshold)
                {
                    goto Converged;
                }
                //After running this a few times, it always seems to converge after around 20 iterations. Choosing 100 is a safe upper bound to prevent memory overflows
                else if (fibreCoords.Count > 100)
                {
                    Console.WriteLine("Couldn't converge in 100 iterations. Most likely stuck in a local maximum");
                    goto Converged;
                }
            }  
            fibrePower fMax = simplexCoords[0];
            fibrePower f1 = simplexCoords[1];
            fibrePower f2 = simplexCoords[2];
            // find the centroid of the first two points
            fibrePower fmid = new fibrePower();
            fmid = findMidpoint(f1, fMax, true);
            // find the reflected point
            fibrePower fref = reflectedPoint(fmid, f2, refScale, true);
            fref.power=getPowerFromCoords(fref,scanData);
            //goes back to the ordering step if the reflected point is the second greatest
            if (fref.power > f1.power && fref.power < fMax.power)
            {
                simplexCoords[1] = fref;
                simplexCoords[2] = f1;
     
                fibreCoords.Add(fref);
            
                goto Order; 
            }
            else if (fref.power > fMax.power)
            {
                goto Expand;
            }
            else
            {
                goto Contract;
            }
        Expand:
            fibrePower fexp = expandedPoint(fmid, fref, expScale, true);
            fexp.power = getPowerFromCoords(fexp, scanData);
            if (fexp.power > fref.power)
            {
                //updates the simplex with the expanded point
                simplexCoords[2] = fexp;
               
            }
            else
            {
                //updates using the reflected point
                simplexCoords[2] = fref;
            }
                   
            fibreCoords.Add(simplexCoords[2]);

            goto Order;
            
        Contract:
            //the contracted point has the same form as expanded but uses either the reflected point or the smallest point in the simplex, whichever is greatest
            if (fref.power > f2.power)
            {
                f2 = fref;
            }
            fibrePower fcon = expandedPoint(fmid, f2, contScale, true);
            fcon.power = getPowerFromCoords(fcon, scanData);
            if (fcon.power > f2.power)
            {
                simplexCoords[2] = fcon;
                    fibreCoords.Add(fcon);
                goto Order;
            }
            else
            {
                goto Shrink;
            }
        Shrink:
            //if none of these are better, we use the highest point, contracted point and a new point to form the simplex
            f1 = shrinkPoint(f1, fMax, shrinkScale, true);
            f2 = shrinkPoint(f2, fMax, shrinkScale, true);
            f1.power = getPowerFromCoords(f1, scanData);
            f2.power = getPowerFromCoords(f2, scanData);
            simplexCoords[1] = f1;
            simplexCoords[2] = f2;

                fibreCoords.Add(f1);
                fibreCoords.Add(f2);
            goto Order;
        Converged:
            timer.Stop();
            outputPower = simplexCoords[0].power;
            outputCoords = simplexCoords[0].coords;
            Console.WriteLine("Fibre aligmnent converged to value "+ outputPower+" in " + timer.ElapsedMilliseconds + " ms and " + fibreCoords.Count + " iterations");
            return outputCoords;
        }

       
        private int[] randomCoords(int maxX, int maxY, Random r)
        {
            //Randomly picks two coordinates between the maximum values
            int[] coords = new int[2];
            
            coords[0] = r.Next(0, maxX);
            coords[1] = r.Next(0, maxY);
            return coords;
        }
        
        private double getPowerFromCoords(fibrePower f1, Double[,] scanData)
        {
            double value;
            try
            {
                value = scanData[f1.coords[0], f1.coords[1]];
            }
            catch
            {
                //typically, it will through an exception if the coordinates are outside the bounds of the data. the simplest way to handle this is to return 0, so the algorithm will not use this point
                value = 0.0;
            }
            return value;
        }
        private fibrePower findMidpoint(fibrePower f1, fibrePower f2, bool debug)
        {
            if (debug)
            {
                int x1 = f1.coords[0];
                int x2 = f2.coords[0];
                int y1 = f1.coords[1];
                int y2 = f2.coords[1];

                int[] newCoords = new int[2];
                newCoords[0] = (x1 + x2) / 2;
                newCoords[1] = (y1 + y2) / 2;

                return new fibrePower(newCoords,0.0);
            }
            else
            {
                //does the same but for doubles
                double x1 = f1.voltages[0];
                double x2 = f2.voltages[0];
                double y1 = f1.voltages[1];
                double y2 = f2.voltages[1];

                double[] newCoords = new double[2];
                newCoords[0] = (x1 + x2) / 2;
                newCoords[1] = (y1 + y2) / 2;

                return new fibrePower(newCoords, 0.0);

            }
        }

        private fibrePower reflectedPoint(fibrePower mid, fibrePower min,double scaling, bool debug)
        {
            if (debug)
            {
                //again, this might cause problems due to integer arithmetic
                int[] refCoords = new int[2];
                int[] midCoords = mid.coords;
                int[] minCoords = min.coords;
                refCoords[0] = (int)(midCoords[0] + scaling * (midCoords[0] - minCoords[0]));
                refCoords[1] = (int)(midCoords[1] + scaling * (midCoords[1] - minCoords[1]));
                return new fibrePower(refCoords, 0.0);
            }
            else
            {
                double[] refVolts = new double[2];
                double[] midVolts = mid.voltages;
                double[] minVolts = min.voltages;
                refVolts[0] = (midVolts[0] + scaling * (midVolts[0] - minVolts[0]));
                refVolts[1] = (midVolts[1] + scaling * (midVolts[1] - minVolts[1]));

                return new fibrePower(refVolts, 0.0);
            }

        }

        private fibrePower expandedPoint(fibrePower mid, fibrePower refl, double scaling, bool debug)
        {
            //this can also work for contraction if we pass the "max" fibrePower as mid and set the scaling to be less than 0.5
            if (debug)
            {
                //again, this might cause problems due to integer arithmetic
                int[] expCoords = new int[2];
                int[] midCoords = mid.coords;
                int[] refCoords = refl.coords;
                expCoords[0] = (int)(midCoords[0] + scaling * (refCoords[0] - midCoords[0]));
                expCoords[1] = (int)(midCoords[1] + scaling * (refCoords[1] - midCoords[1]));
                return new fibrePower(expCoords, 0.0);
            }
            else
            {
                double[] expVolts = new double[2];
                double[] midVolts = mid.voltages;
                double[] refVolts = refl.voltages;
                expVolts[0] = (midVolts[0] + scaling * (refVolts[0] - midVolts[0]));
                expVolts[1] = (midVolts[1] + scaling * (refVolts[1] - midVolts[1]));

                return new fibrePower(expVolts, 0.0);
            }
        }

        private fibrePower shrinkPoint(fibrePower point, fibrePower max, double scaling, bool debug)
        {
            //Shrinks the point based on the scaling value passed. This happens in the rare case thatThis just modifies the coordinates - elsewhere the power at the new point will be obtained.
            if (debug)
            {             
                point.coords[0] = (int)(max.coords[0] + scaling * (point.coords[0] - max.coords[0]));
                point.coords[1] = (int)(max.coords[1] + scaling * (point.coords[1] - max.coords[1]));
            
            }
            else
            {
                point.voltages[0] = (max.voltages[0] + scaling * (point.voltages[0] - max.voltages[0]));
                point.voltages[1] = (max.voltages[1] + scaling * (point.voltages[1] - max.voltages[1]));
            }

            return point;
        }

 
        public void loadFibreScanData(string path)
        {
            StreamReader reader = new StreamReader(path);
            var lines = new List<double[]>();
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                //Removes the last element if it is not a number
                if (line[line.Length-1] == "") { line = line.Take(line.Count() - 1).ToArray(); }
                double[] values = Array.ConvertAll(line, Double.Parse);
                lines.Add(values);
            }
            var data = lines.ToArray();
            //gets the max value for normalisation
            List<double> maxVals = new List<double>();
            foreach (double[] vals in data)
                maxVals.Add(vals.Max());
            double max = maxVals.Max();
            //Converts the data from a jagged array to a multidimensional array
            Double[,] data2d = new Double[data.Length, data.Max(x => x.Length)];

            for (var i = 0; i < data.Length; i++)
            {
                for (var j = 0; j < data[i].Length; j++)
                {
                    data2d[i, j] = data[i][j]/max;
                }
            }
            scanData = data2d;
        }

        public IEnumerable<double> fibrePowers
        {
            get
            { return fibreCoords.Select(d => d.power); }
        }

        public double[,] ScanData
        {
            get { return scanData; }
            set { scanData = value; }
        }
        //Calculates the standard deviation of a list of doubles
        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
    }
}
