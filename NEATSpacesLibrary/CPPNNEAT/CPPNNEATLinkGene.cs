using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATLinkGene
    {
        public int InnovationNumber
        {
            get;
            private set;
        }

        public bool Enabled;

        public CPPNNEATLinkGene(CPPNNEATNeuronGene cPPNNEATNeuronGene, CPPNNEATNeuronGene cPPNNEATNeuronGene_2)
        {
        }

        public double Weight { get; set; }
    }
}
