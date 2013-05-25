using System;
using System.Numerics;
using DotNetExtensions;

namespace ComplexCPPNNEAT
{
    public static class CPPNActivationFunctionFactories
    {
        public const int MIN_POWER = 2;
        public const int MAX_POWER = 8;

        public static CPPNNEATActivationFunction ComplexLinearActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => x, "x");
        }

        public static CPPNNEATActivationFunction ComplexPolynomialActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Pow(x, power),
                                                                String.Format("x^{0}", power));
        }

        public static CPPNNEATActivationFunction ComplexLogarithmicActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Log(x, power),
                                                                String.Format("log{0}(x)", power));
        }

        public static CPPNNEATActivationFunction ComplexExponentialActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Pow(power, x),
                                                                        String.Format("{0}^x", power));
        }

        public static CPPNNEATActivationFunction ComplexSteepenedSigmoidActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => 1 / (1 + Complex.Exp(-4.9 * x)), "sig(x)");
        }

        public static CPPNNEATActivationFunction ComplexGaussianActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Exp(-Complex.Pow(x * 2.5, 2.0)), "gauss(x)");
        }

        public static CPPNNEATActivationFunction ComplexSinActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Sin(x), "sin(x)");
        }

        public static CPPNNEATActivationFunction ComplexEulerActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Exp(x), "e^x");
        }

        public static CPPNNEATActivationFunction ComplexTanHActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Tanh(x), "tanh(x)");
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