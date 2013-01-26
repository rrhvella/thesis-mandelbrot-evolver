using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using CPPNNEAT.Extensions;

namespace CPPNNEAT.CPPNNEAT
{
    /// <summary>
    /// Stores a set of activation function factories as static methods.
    /// </summary>
    public static class CPPNActivationFunctionFactories
    {
        /// <summary>
        /// In the following comments, this corresponds to the minimum value of a.
        /// </summary>
        public const int MIN_POWER = 2;
        /// <summary>
        /// In the following comments, this corresponds to the maximum value of a.
        /// </summary>
        public const int MAX_POWER = 8;

        /// <summary>
        /// f(x) = x
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexLinearActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => x, "x"); 
        }

        /// <summary>
        /// f(x) = x^a.
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexPolynomialActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Pow(x, power), 
                                                                String.Format("x^{0}", power)); 
        }

        /// <summary>
        /// f(x) = log_a(x).
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexLogarithmicActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Log(x, power), 
                                                                String.Format("log{0}(x)", power)); 
        }

        /// <summary>
        /// f(x) = a^x.
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexExponentialActivationFunctionFactory()
        {
            var power = MathExtensions.RandomInteger(MIN_POWER, MAX_POWER);
            return new CPPNNEATActivationFunction(x => Complex.Pow(power, x),
                                                                        String.Format("{0}^x", power));
        }

        /// <summary>
        /// f(x) = 1/(1 + e^4.9x).
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexSteepenedSigmoidActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => 1 / (1 + Complex.Exp(-4.9 * x)), "sig(x)"); 
        }

        /// <summary>
        /// f(x) = e^(-6.26x^2).
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexGaussianActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Exp(-Complex.Pow(x * 2.5, 2.0)), "gauss(x)"); 
        }

        /// <summary>
        /// f(x) = sin(x).
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexSinActivationFunctionFactory()
        {
            return new CPPNNEATActivationFunction(x => Complex.Sin(x), "sin(x)"); 
        }

        /// <summary>
        /// f(x) = e^x.
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexEulerActivationFunctionFactory ()
        {
            return new CPPNNEATActivationFunction(x => Complex.Exp(x), "e^x");
        }

        /// <summary>
        /// f(x) = tanh(x).
        /// </summary>
        /// <returns></returns>
        public static CPPNNEATActivationFunction ComplexTanHActivationFunctionFactory ()
        {
            return new CPPNNEATActivationFunction(x => Complex.Tanh(x), "tanh(x)"); 
        }
    }

    /// <summary>
    /// Wraps an activation function used in the CPPN-NEAT algorithm.
    /// </summary>
    public class CPPNNEATActivationFunction
    {
        /// <summary>
        /// The activation function.
        /// </summary>
        public Func<Complex, Complex> Function
        {
            get;
            private set;
        }

        /// <summary>
        /// The label of the function.
        /// </summary>
        private string label;
        public string Label
        {
            get
            {
                return label;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="function">The activation function.</param>
        /// <param name="label">The label of the function.</param>
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
