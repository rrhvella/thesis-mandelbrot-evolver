using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public static class CPPNActivationFunctions
    {
        public static Func<double, double> LinearActivationFunction 
        {
            get
            {
                return x => x;
            }
        }

        public static Func<double, double> TanHActivationFunction 
        {
            get
            {
                return x => Math.Tanh(x);
            }
        }

        public static Func<double, double> SinActivationFunction 
        {
            get
            {
                return x => Math.Sin(x);
            }
        }

        public static Func<double, double> GaussActivationFunction 
        {
            get
            {
                return x => 2 * Math.Exp(-(Math.Pow(x, 2)/18)) - 1;
            }
        }

        public static Func<double, double> SteepenedSigmoidActivationFunction
        {
            get
            {
                return x => 1 / (1 + Math.Exp(-4.9 * x));
            }
        }
    }

    public abstract class CPPNNetworkNeuron
    {
        public abstract double Activation
        {
            get;
        }
    }

    public class CPPNBiasNeuron: CPPNNetworkNeuron
    {
        public override double Activation
        {
            get 
            {
                return 1.0;
            }
        }
    }

    public class CPPNInputNeuron: CPPNNetworkNeuron
    {
        private double activation;
        public override double Activation
        {
            get 
            {
                return activation;
            }
        }

        public void SetInput(double input)
        {
            activation = input;
        }
    }

    public class CPPNOutputNeuron: CPPNNetworkNeuron
    {
        private class Synapse
        {
            public CPPNNetworkNeuron Neuron
            {
                get;
                private set;
            }

            public double Weight
            {
                get;
                private set;
            }

            public Synapse(CPPNNetworkNeuron neuron, double weight)
            {
                this.Neuron = neuron;
                this.Weight = weight;
            }
        }

        private List<Synapse> synapsis;
        private Func<double, double> activationFunction;

        private bool isCalculating;
        private double previousActivation;

        public override double Activation
        {
            get 
            {
                if (isCalculating)
                {
                    return previousActivation;
                }

                isCalculating = true;

                var net = (from synapse in synapsis
                           select synapse.Neuron.Activation * synapse.Weight).Sum();

                var activation = activationFunction(net);

                isCalculating = false;

                previousActivation = activation;

                return activation;
            }
        }

        public CPPNOutputNeuron(Func<double, double> activationFunction)
        {
            this.activationFunction = activationFunction;
            this.synapsis = new List<Synapse>();
            this.isCalculating = false;
        }

        public void AddChild(CPPNNetworkNeuron neuron, double weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }
    }

    public class CPPNHiddenNeuron: CPPNOutputNeuron
    {
        public CPPNHiddenNeuron(Func<double, double> activationFunction): base(activationFunction)
        {
        }
    }
}
