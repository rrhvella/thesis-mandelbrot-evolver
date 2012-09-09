using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATNeuronGene
    {
        public Func<double[], double> ActivationFunction
        {
            get;
            private set;
        }
    }
}
