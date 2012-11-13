﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

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

        public static Func<double, double> ClippedLinearActivationFunction 
        {
            get
            {
                return x => (x > 1)? 1 : (x < -1)? -1 : x;
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
                return x => Math.Sin(2 * x);
            }
        }

        public static Func<double, double> GaussActivationFunction 
        {
            get
            {
                return x => Math.Exp(-Math.Pow(x * 2.5, 2.0));
            }
        }

        public static Func<double, double> StepActivationFunction 
        {
            get
            {
                return x => (x >= 0)? 1 : 0;
            }
        }

        public static Func<double, double> SteepenedSigmoidActivationFunction
        {
            get
            {
                return x => 1 / (1 + Math.Exp(-4.9 * x));
            }
        }

        public static Func<Complex, Complex> ComplexSteepenedSigmoidActivationFunction 
        {
            get
            {
                return x => 1 / (1 + Complex.Exp(-4.9 * x));
            }
        }

        public static Func<Complex, Complex> ComplexLinearActivationFunction 
        {
            get
            {
                return x => x;
            }
        }

        public static Func<Complex, Complex> ComplexGaussianActivationFunction 
        {
            get
            {
                return x => Complex.Exp(-Complex.Pow(x * 2.5, 2.0));
            }
        }

        public static Func<Complex, Complex> ComplexTanHActivationFunction
        {
            get
            {
                return x => Complex.Tanh(x);
            }
        }

        public static Func<Complex, Complex> ComplexSinActivationFunction
        {
            get
            {
                return x => Complex.Sin(x);
            }
        }
    }

    public class Synapse
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

        public void AddChild(CPPNNetworkNeuron neuron, double weight)
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
