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

        public CPPNNEATLinkGene(int innovationNumber, CPPNNEATNeuronGene from, CPPNNEATNeuronGene to, double weight)
        {
            this.InnovationNumber = innovationNumber;
            this.Enabled = true;

            this.From = from;
            this.To = to;

            this.Weight = weight;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public double Weight 
        { 
            get; 
            set; 
        }

        public CPPNNEATNeuronGene From 
        { 
            get; 
            private set; 
        }

        public CPPNNEATNeuronGene To 
        { 
            get; 
            private set; 
        }
    }
}
