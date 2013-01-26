using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPPNNEAT.GeneticAlgorithms;
using System.Numerics;

namespace CPPNNEAT.CPPNNEAT
{
    /// <summary>
    /// Represents a link in CPPN-NEAT gene sequence.
    /// </summary>
    public class CPPNNEATLinkGene
    {
        /// <summary>
        /// The innovation number, representing the historical marking of the link gene.
        /// </summary>
        public int InnovationNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="innovationNumber">The historical marking of the link gene.</param>
        /// <param name="from">The neuron from which the link emerges.</param>
        /// <param name="to">The neuron the link is entering.</param>
        /// <param name="weight">The weight which amplifies signals travelling through the link
        /// </param>
        public CPPNNEATLinkGene(int innovationNumber, CPPNNEATNeuronGene from, 
                            CPPNNEATNeuronGene to, Complex weight)
        {
            this.InnovationNumber = innovationNumber;
            this.Enabled = true;

            this.From = from;
            this.To = to;

            this.Weight = weight;
        }

        /// <summary>
        /// Is true if this gene is active.
        /// </summary>
        public bool Enabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// The weight which amplifies signals travelling through the link.
        /// </summary>
        internal Complex Weight 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// The neuron from which the link emerges.
        /// </summary>
        public CPPNNEATNeuronGene From 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The neuron the link is entering.</param>
        /// </summary>
        public CPPNNEATNeuronGene To 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Returns a copy of this link gene.
        /// </summary>
        /// <returns></returns>
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
