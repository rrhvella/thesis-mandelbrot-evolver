using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATLinkGene
    {
        public int InnovationNumber
        {
            get;
            private set;
        }

        public CPPNNEATLinkGene(int innovationNumber, CPPNNEATNeuronGene from, CPPNNEATNeuronGene to, Complex weight)
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
            internal set;
        }

        internal Complex Weight 
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

        public CPPNNEATLinkGene Copy()
        {
            return (CPPNNEATLinkGene)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return String.Format("{0} - {1:f2} -> {2}", From, Weight, To);
        }
    }
}
