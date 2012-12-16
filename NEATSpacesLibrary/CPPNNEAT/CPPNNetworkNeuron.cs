using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// Represents a connection to another neuron.
    /// </summary>
    public class Synapse
    {
        /// <summary>
        /// The other neuron this synapse is connected to.
        /// </summary>
        public CPPNNetworkNeuron Neuron
        {
            get;
            private set;
        }

        /// <summary>
        /// The weight of this synapse.
        /// </summary>
        public Complex Weight
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="neuron">The other neuron this synapse is connected to.</param>
        /// <param name="weight">The weight of this synapse.</param>
        public Synapse(CPPNNetworkNeuron neuron, Complex weight)
        {
            this.Neuron = neuron;
            this.Weight = weight;
        }
    }

    /// <summary>
    /// Represents a neuron in a CPPN network.
    /// </summary>
    public abstract class CPPNNetworkNeuron
    {
        /// <summary>
        /// The function which determines the strength of the neuron's activation.
        /// </summary>
        public Func<Complex, Complex> ActivationFunction
        {
            get;
            protected set;
        }

        /// <summary>
        /// The function of the neuron within the network.
        /// </summary>
        public CPPNNeuronType NeuronType
        {
            get;
            protected set;
        }

        /// <summary>
        /// The synapsis connecting this neuron to the other neurons.
        /// </summary>
        protected List<Synapse> synapsis;
        public IEnumerable<Synapse> Synapsis
        {
            get
            {
                return synapsis.AsReadOnly();
            }
        }

        public CPPNNetworkNeuron()
        {
            synapsis = new List<Synapse>();
        }

        /// <summary>
        /// Creates a connection from the given neuron to this neuron with the given weight.
        /// </summary>
        /// <param name="neuron"></param>
        /// <param name="weight"></param>
        public void AddChild(CPPNNetworkNeuron neuron, Complex weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }
    }

    /// <summary>
    /// CPPN bias neuron.
    /// </summary>
    public class CPPNBiasNeuron: CPPNNetworkNeuron
    {
        public CPPNBiasNeuron()
        {
            NeuronType = CPPNNeuronType.Bias;
        }
    }

    /// <summary>
    /// CPPN input neuron.
    /// </summary>
    public class CPPNInputNeuron: CPPNNetworkNeuron
    {
        public CPPNInputNeuron()
        {
            NeuronType = CPPNNeuronType.Input;
        }
    }

    /// <summary>
    /// CPPN output neuron.
    /// </summary>
    public class CPPNOutputNeuron: CPPNNetworkNeuron
    {
        public CPPNOutputNeuron(Func<Complex, Complex> activationFunction)
        {
            this.ActivationFunction = activationFunction;
            this.NeuronType = CPPNNeuronType.Output;
        }
    }

    /// <summary>
    /// CPPN hidden neuron.
    /// </summary>
    public class CPPNHiddenNeuron: CPPNOutputNeuron
    {
        public CPPNHiddenNeuron(Func<Complex, Complex> activationFunction): base(activationFunction)
        {
            this.NeuronType = CPPNNeuronType.Hidden;
        }
    }
}
