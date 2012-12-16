using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// Classifies neurons by their function with a network.
    /// </summary>
    public enum CPPNNeuronType
    {
        Bias = 0,
        Input,
        Hidden,
        Output
    }

    /// <summary>
    /// A neuron belonging to the links in a CPPN-NEAT gene sequence.
    /// </summary>
    public class CPPNNEATNeuronGene 
    {
        /// <summary>
        /// The innovation number, representing the historical marking of the neuron gene.
        /// </summary>
        private int innovationNumber;

        /// <summary>
        /// The layer level of the neuron. This is used to enforce feed forward networks.
        /// </summary>
        public double Level
        {
            get;
            private set;
        }

        /// <summary>
        /// The function of the neuron inside the network.
        /// </summary>
        public CPPNNeuronType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="innovationNumber">The innovation number, representing the historical marking of the neuron gene.
        /// </param>
        /// <param name="level">The layer level of the neuron.</param>
        /// <param name="type">The function of the neuron inside the network</param>
        /// <param name="activationFunction">The function which determines the strength of the signal the neuron fires.
        /// </param>
        public CPPNNEATNeuronGene(int innovationNumber, double level, 
                                CPPNNeuronType type, CPPNNEATActivationFunction activationFunction)
        {
            this.innovationNumber = innovationNumber;
            this.Level = level;

            this.Type = type;
            this.ActivationFunction = activationFunction;
        }

        /// <summary>
        /// The function which determines the strength of the signal the neuron fires.
        /// </summary>
        public CPPNNEATActivationFunction ActivationFunction
        {
            get;
            internal set;
        }

        /// <summary>
        /// Update the phene for this neuron gene.
        /// </summary>
        public void Update()
        {
            switch (Type)
            {
                case CPPNNeuronType.Input:
                    Phene = new CPPNInputNeuron();
                    break;

                case CPPNNeuronType.Output:
                    Phene = new CPPNOutputNeuron(ActivationFunction.Function);
                    break;

                case CPPNNeuronType.Hidden:
                    Phene = new CPPNHiddenNeuron(ActivationFunction.Function);
                    break;

                case CPPNNeuronType.Bias:
                    Phene = new CPPNBiasNeuron();
                    break;
            }
        }

        /// <summary>
        /// The phene generated from this neuron.
        /// </summary>
        public CPPNNetworkNeuron Phene 
        { 
            get; 
            private set; 
        }
        
        /// <summary>
        /// Returns the string representing the neuron's type.
        /// </summary>
        /// <returns></returns>
        public string TypeString() 
        {
            switch (Type)
            {
                case CPPNNeuronType.Bias:
                    return "B";

                case CPPNNeuronType.Output:
                    return "O";

                case CPPNNeuronType.Input:
                    return "I";

                case CPPNNeuronType.Hidden:
                    return "H";

                default:
                    return "";
            }
        }

        public override string ToString()
        {
            if(Type == CPPNNeuronType.Bias || Type == CPPNNeuronType.Input)  
            {
                return String.Format("{0}({1})", TypeString(), innovationNumber);
            }

            return String.Format("{0}({1}, {2})", TypeString(), innovationNumber, ActivationFunction);
        }
    }
}
