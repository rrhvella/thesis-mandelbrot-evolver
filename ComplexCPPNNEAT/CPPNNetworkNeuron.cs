using System;
using System.Collections.Generic;
using System.Numerics;

namespace CPPNNEAT.CPPNNEAT
{
    public abstract class CPPNNetworkNeuron
    {
        public abstract Complex Activation
        {
            get;
        }
    }

    public class CPPNBiasNeuron : CPPNNetworkNeuron
    {
        public override Complex Activation
        {
            get
            {
                return 1.0;
            }
        }
    }

    public class CPPNInputNeuron : CPPNNetworkNeuron
    {
        private Complex activation;

        public override Complex Activation
        {
            get
            {
                return activation;
            }
        }

        public void SetInput(Complex activationValue)
        {
            activation = activationValue;
        }
    }

    public class CPPNOutputNeuron : CPPNNetworkNeuron
    {
        private class Synapse
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

        private List<Synapse> synapsis;

        private Func<Complex, Complex> activationFunction;

        private bool isCalculating;

        private Complex previousActivation;

        public override Complex Activation
        {
            get
            {
                if (isCalculating)
                {
                    return previousActivation;
                }

                isCalculating = true;

                Complex net = 0;

                foreach (var synapse in synapsis)
                {
                    net += synapse.Neuron.Activation * synapse.Weight;
                }

                var activation = activationFunction(net);

                isCalculating = false;

                previousActivation = activation;

                return activation;
            }
        }

        public CPPNOutputNeuron(Func<Complex, Complex> activationFunction)
        {
            this.activationFunction = activationFunction;
            this.synapsis = new List<Synapse>();
            this.isCalculating = false;
        }

        public void AddChild(CPPNNetworkNeuron neuron, Complex weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }

        public void Reset()
        {
            previousActivation = 0;
        }
    }

    public class CPPNHiddenNeuron : CPPNOutputNeuron
    {
        public CPPNHiddenNeuron(Func<Complex, Complex> activationFunction)
            : base(activationFunction)
        {
        }
    }
}