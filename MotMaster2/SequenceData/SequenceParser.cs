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
            if (value == "") return 0.0;
            bool result = Double.TryParse(value, out number);
            if (result) return number;
            else return Convert.ToDouble(Controller.sequenceData.Parameters[value].Value);
        }
        
        //TODO tidy up this to check based on the raw string
        public static bool CheckMuquans(string command)
        {
            return true;
       
        }
           
    }
}
