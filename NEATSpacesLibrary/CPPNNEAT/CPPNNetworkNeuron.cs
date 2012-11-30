using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public static class CPPNActivationFunctionFactories
    {
        public const int MIN_POWER = 2;
        public const int MAX_POWER = 4;

        public static Func<Func<Complex, Complex>> ComplexLinearActivationFunctionFactory
        {
            get
            {
                return delegate() { return x => x; };
            }
        }

        public static Func<Func<Complex, Complex>> ComplexPolynomialActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return x => Complex.Pow(x, power); };
            }
        }

        public static Func<Func<Complex, Complex>> ComplexLogarithmicActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return x => Complex.Log(x, power); };
            }
        }

        public static Func<Func<Complex, Complex>> ComplexExponentialActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return x => Complex.Pow(power, x); };
            }
        }
        public static Func<Func<Complex, Complex>> ComplexSteepenedSigmoidActivationFunctionFactory
        {
            get
            {
                return delegate() { return x => 1 / (1 + Complex.Exp(-4.9 * x)); };
            }
        }
        public static Func<Func<Complex, Complex>> ComplexGaussianActivationFunctionFactory
        {
            get
            {
                return delegate() { return x => Complex.Exp(-Complex.Pow(x * 2.5, 2.0)); };
            }
        }

        public static Func<Func<Complex, Complex>> ComplexSinActivationFunctionFactory
        {
            get
            {
                return delegate() { return x => Complex.Sin(x); };
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
