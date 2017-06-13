using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotMath;

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

        //TODO: Make the parser work for strings which represent Script parameter names
        internal bool CheckMuquans(string command)
        {
            if (command == "") return true;
            string[] values;
            if (!command.Contains(',')) values = command.Split(' ');
            else values = command.Split(',');
            if (values[0] == "Set")
            {
                double result;
                if (values.Length != 2) throw new Exception("Incorrect number of arguments for Set");
                else if (!Double.TryParse(values[1], out result)) throw new Exception("Set value is not a number");
                else return true;
            }
            else if (values[0] == "Sweep")
            {
                double result;
                if (values.Length != 3) throw new Exception("Incorrect number of arguments for Sweep");
                else if (!Double.TryParse(values[1], out result)) throw new Exception("Sweep value is not a number");
                else if (!Double.TryParse(values[2], out result)) throw new Exception("Sweep time is not a number");
                else return true;
            }
            else
            {
                throw new Exception("Incorrect command - needs to be Set or Sweep");
            }
        }
           
    }
}
