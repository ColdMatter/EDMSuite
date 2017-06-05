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

    }
}
