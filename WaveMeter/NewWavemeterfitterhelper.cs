
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Analyze;
using IMAQ;
using System.IO;

namespace WaveMeter
{
    class NewWavemeterfitterhelper
    {

        private static SumGaussianFitter SumGaussian;
        public double[] Datapoints;
        public double[] Position;
        public double[] imagedata;







        public NewWavemeterfitterhelper()
        {
            SumGaussian = new SumGaussianFitter();
            
        }


       
        private double findminimum()
        {
            double min=imagedata[0];
            for(int i=0; i < imagedata.GetLength(0);i++)
                {
                    if (imagedata[i]<min)
                    {
                        min =imagedata[i];

                    }
                }
            return min;

        }
        

        private double findmaximum()
        {
            double max = 0;
            for (int i = 0; i < imagedata.GetLength(0); i++)
            {
                if (imagedata[i] > max)
                {
                    max = imagedata[i];

                }
            }
            return max;

        }

        private int findcenter()
        {
            int position = 0;
            double maximum = findmaximum();
            for (int i=0; i<imagedata.GetLength(0);i++)
            {
                if (imagedata[i]==maximum)
                {
                    position = i;
                }
            }
            return position;

        }

     

       public double fndwidth()
        {
            int initialposition = 0;
            int finalposition = 0;
            double maximum=findmaximum();
            double minimum=findminimum();


            int center = findcenter();
            int width = 0;
            for (int i=center; i<imagedata.GetLength(0);i++)
             {
                 if (imagedata[i]<(minimum+(maximum-minimum)/2))
                    {
                        finalposition=i;
                        break;

                    }
           
            }
           
           
           for (int i=center;i>0;i--)
            {
                if (imagedata[i] <(minimum + (maximum - minimum) / 2))
                {
                   initialposition=i;
                   break;

                }    
            }


           width = finalposition - initialposition;
           return width;
            
        }


        public double [] Guesstheparameters()
        {
            
            double[] initParameters={0,0,0,0,0,0,0,0};

            initParameters[0] = findminimum();
            initParameters[1] = findmaximum();
            initParameters[2] = findcenter();
            initParameters[3] = fndwidth();

            int center = (int)(initParameters[2]);
            int width = (int)(initParameters[3]);

            for (int i = center - width; i < center + width;i++)
            {
                imagedata[i] = 0;
            }



                initParameters[4] = findminimum();
            initParameters[5] = findmaximum();
            initParameters[6] = findcenter();
            initParameters[7] = fndwidth();

            Console.WriteLine(initParameters[0]);
            Console.WriteLine(initParameters[1]);
            Console.WriteLine(initParameters[2]);
            Console.WriteLine(initParameters[3]);
            Console.WriteLine(initParameters[4]);
            Console.WriteLine(initParameters[5]);
            Console.WriteLine(initParameters[6]);
            Console.WriteLine(initParameters[7]);
      

            return initParameters;

         

        }  

        
       
        
        
        public double[] Fittedvalues ()
        {

            imagedata = Datapoints;


            SumGaussian.Fit(Position, Datapoints, Guesstheparameters());



            outputParameters(SumGaussian.ParameterReport);


            return SumGaussian.FittedValues;

        }

        public void Fittedparameter()
        {
            imagedata = Datapoints;
            
            SumGaussian.Fit(Position, Datapoints, Guesstheparameters());
         
            outputParameters(SumGaussian.ParameterReport);
            
            

        }



     public double[] Fitting()
        {

            double[] FitX = new double[2000];
            double[] FitY = new double[2000];

            double[] paramaters = Guesstheparameters();
            int center = (int)(paramaters[2]);
            int width = (int)(paramaters[3]);


            for (int i = center - 5 * width; i < (center + 5 * width); i++)
            {
                FitY[i] = Datapoints[i];
                FitX[i] = Position[i];


            }




            SumGaussian.Fit(FitX, FitY, Guesstheparameters());



            outputParameters(SumGaussian.ParameterReport);



            return (SumGaussian.FittedValues);

            

        }

    






        public static void outputParameters(string text)
        {
            string path = @"C:\Users\Andy\Desktop\Report.txt";

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string[] createText = { "Time and Fitting Parameters" };
                File.WriteAllLines(path, createText, Encoding.UTF8);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = text + Environment.NewLine;
            File.AppendAllText(path, appendText, Encoding.UTF8);

            // Open the file to read from.
           // string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            //foreach (string s in readText)
            //{
             //   Console.WriteLine(s);
           // }
        }

    }
}
