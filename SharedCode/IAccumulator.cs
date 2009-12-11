using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis
{
    public interface IAccumulator<T>
    {
        void Add(T val);
        T GetResult();
    }
}
