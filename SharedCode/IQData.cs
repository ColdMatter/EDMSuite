using System;
using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// This is a class which represents a collection of the I/Q arrays needed to write
    /// an arbitray waveform to the NI RF signal generators.
    /// It consists of write methods.
    /// </summary>
    public class IQData
    {
        private double[] _iData;
        private double[] _qData;

        public IQData(int length)
        {
            _iData = new double[length];
            _qData = new double[length];
        }

        public double[] IData
        {
            get
            {
                return _iData;
            }
        }

        public double[] QData
        {
            get
            {
                return _qData;
            }
        }

        public void WriteIQData(int index, double iVal, double qVal)
        {
            try
            {
                _iData[index] = iVal;
                _qData[index] = qVal;
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Index out of bounds!");
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Array not initialised properly!");
            }
            catch (Exception e)
            {
                throw new Exception("Some other bug occured", e);
            }
        }

    }
}
