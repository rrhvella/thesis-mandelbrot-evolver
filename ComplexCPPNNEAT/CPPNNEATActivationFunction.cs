/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
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