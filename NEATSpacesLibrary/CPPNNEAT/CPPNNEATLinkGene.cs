using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATLinkGene: IDebugabble
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

        public string DebugInformation()
        {
            return String.Format("[{0}, {1}, {2:f2}, {3}->{4}]", InnovationNumber, (Enabled)? 1 : 0, Weight,
                                                From.DebugInformation(), To.DebugInformation());

        }
    }
}
