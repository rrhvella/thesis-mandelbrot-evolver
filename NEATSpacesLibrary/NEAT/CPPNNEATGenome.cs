using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.NEAT
{
    public abstract class CPPNNEATGenome<T> : Genome<T>
    {
        public double GetActivation(double[] input)
        {
            return 0;
        }
    }
}
