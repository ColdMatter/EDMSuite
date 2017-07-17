using dotMath;
using System;
using System.Linq;

namespace MOTMaster2.SequenceData
{
    /// <summary>
    /// This class is used to check the validity of arguments for analog channel values
    /// </summary>
    class SequenceParser
    {
        public bool CheckNumber(string analogString, out double value)
        {
           return Double.TryParse(analogString,out value);
        }

        public bool CheckFunction(string analogFunction)
        {
            EqCompiler compiler = new EqCompiler(analogFunction,true);
            try 
	        {	        
		        compiler.Compile();
    	    }
	        catch (Exception e)
	        {
		        return false;
	        }
            return true;
        }

        public static double ParseOrGetParameter(string value)
        {
            double number = 0.0;
            if (value == "") return number;
            bool result = Double.TryParse(value, out number);
            if (result) return number;
            else return (double)Controller.sequenceData.Parameters.Where(t => t.Name == value).Select(t => t.Value).First();
        }
        
        //TODO tidy up this to check based on the raw string
        public static bool CheckMuquans(string command)
        {
            if (command == "") return true;
            string[] values;
            if (!command.Contains(',')) values = command.Split(' ');
            else values = command.Split(' ');
            if (values[0] == "set")
            {
                if (values.Length != 2) throw new Exception("Incorrect number of arguments for Set");
                else
                {
                    try
                    {
                        ParseOrGetParameter(values[1]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Set value is not a number");
                    }
                }
                    return true;
            }
            else if (values[0] == "Sweep")
            {
                if (values.Length != 3) throw new Exception("Incorrect number of arguments for Sweep");
                else {
                    try
                    { 
                        ParseOrGetParameter(values[1]);
                    } 
                    catch(Exception e)
                    { throw new Exception("Sweep value is not a number");
                    }
                    try
                    { 
                        ParseOrGetParameter(values[2]);
                    } 
                    catch(Exception e)
                    { 
                        throw new Exception("Sweep value is not a number");
                    }
                    return true;
                }
            }
            else
            {
                throw new Exception("Incorrect command - needs to be Set or Sweep");
            }
        }
           
    }
}
