using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class Synapse
    {
        public CPPNNetworkNeuron Neuron
        {
            get;
            private set;
        }

        public Complex Weight
        {
            get;
            private set;
        }

        public Synapse(CPPNNetworkNeuron neuron, Complex weight)
        {
            this.Neuron = neuron;
            this.Weight = weight;
        }
    }

    public abstract class CPPNNetworkNeuron
    {
        public Func<Complex, Complex> ActivationFunction
        {
            get;
            protected set;
        }

        public CPPNNeuronType NeuronType
        {
            get;
            protected set;
        }

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

        public void AddChild(CPPNNetworkNeuron neuron, Complex weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }
    }

    public class CPPNBiasNeuron: CPPNNetworkNeuron
    {
        public CPPNBiasNeuron()
        {
            NeuronType = CPPNNeuronType.Bias;
        }

    }

    public class CPPNInputNeuron: CPPNNetworkNeuron
    {
        public CPPNInputNeuron()
        {
            NeuronType = CPPNNeuronType.Input;
        }
    }

    public class CPPNOutputNeuron: CPPNNetworkNeuron
    {
        public CPPNOutputNeuron(Func<Complex, Complex> activationFunction)
        {
            this.ActivationFunction = activationFunction;
            this.NeuronType = CPPNNeuronType.Output;
        }
    }

    public class CPPNHiddenNeuron: CPPNOutputNeuron
    {
        public CPPNHiddenNeuron(Func<Complex, Complex> activationFunction): base(activationFunction)
        {
            this.NeuronType = CPPNNeuronType.Hidden;
        }
    }
}
