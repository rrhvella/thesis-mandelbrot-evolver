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

        public static Func<CPPNNEATActivationFunction> ComplexLinearActivationFunctionFactory
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => x, "x"); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexPolynomialActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Pow(x, power), 
                                                                        String.Format("x^{0}", power)); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexLogarithmicActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Log(x, power), 
                                                                        String.Format("log{0}(x)", power)); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexExponentialActivationFunctionFactory
        {
            get
            {
                var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Pow(power, x), 
                                                                        String.Format("{0}^x", power)); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexSteepenedSigmoidActivationFunctionFactory
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => 1 / (1 + Complex.Exp(-4.9 * x)), "σ(x)"); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexGaussianActivationFunctionFactory
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Exp(-Complex.Pow(x * 2.5, 2.0)), "∮(x)"); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexSinActivationFunctionFactory
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Sin(x), "sin(x)"); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexEulerActivationFunctionFactory 
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Exp(x), "e^x"); };
            }
        }

        public static Func<CPPNNEATActivationFunction> ComplexTanHActivationFunctionFactory 
        {
            get
            {
                return delegate() { return new CPPNNEATActivationFunction(x => Complex.Tanh(x), "tanh(x)"); };
            }
        }
    }

    public class CPPNNEATActivationFunction
    {
        public Func<Complex, Complex> Function
        {
            get;
            private set;
        }

        private string label;

        public string Label
        {
            get
            {
                return label;
            }
        }

        public CPPNNEATActivationFunction(Func<Complex, Complex> function, string label)
        {
            this.Function = function;
            this.label = label;
        }

        public override string ToString()
        {
            return label;
        }
    }
}
