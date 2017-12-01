using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferCavityLock2012
{
    public class LorentzianFit
    {
        public double Background;
        public double Amplitude;
        public double Centre;
        public double Width;

        public LorentzianFit(double background, double amplitude, double centre, double width)
        {
            Background = background;
            Amplitude = amplitude;
            Centre = centre;
            Width = width;
        }
    }
}
