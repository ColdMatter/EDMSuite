
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
    
    class WavemeterFitterHelper
    {
        private static GaussianFitter gaussian;
        public double[] Datapoints;
        public double[] Position;
        public double[] imagedata;




        public WavemeterFitterHelper()
        {
            gaussian = new GaussianFitter();
            
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

     

       public double findwidth()
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
            
            double[] initParameters={0,0,0,0};

            initParameters[0] = findminimum();
            initParameters[1] = findmaximum();
            initParameters[2] = findcenter();
            initParameters[3] = findwidth();




            return initParameters;




        }  

        
        public double[] Fittedvalues ()
        {

            imagedata = Datapoints;


            gaussian.Fit(Position, imagedata, Guesstheparameters());





            return gaussian.FittedValues;

        }

        public void Fittedparameter()
        {
            imagedata = Datapoints;
            
            gaussian.Fit(Position, imagedata, Guesstheparameters());
         
            
            

        }

        public void datamassage()
        {
            imagedata = Datapoints;
            int center = findcenter();
            int width = (int)findwidth();

            for (int i = center - 2*width; i < center +2*width; i++)
            {
                imagedata[i] = 0;
            }


            
        }

        public double center()
        {
            return gaussian.returncenter();
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


