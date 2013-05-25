using System;
using System.Collections.Generic;
using System.Numerics;
using ComplexCPPNNEAT;

namespace MandelbrotCPPNNEAT
{
    public class MandelbrotCPPNNEATGA : BaseCPPNNEATGA<MandelbrotCPPNNEATGenome, MandelbrotCPPNNEATPhenome>, ICPPNNEATGA
    {
        private const int NUMBER_OF_INPUTS = 2;

        public Complex ViewPosition
        {
            get;
            set;
        }

        public double ViewScale
        {
            get;
            set;
        }

        public MandelbrotCPPNNEATGA(int populationSize,
Func<MandelbrotCPPNNEATGenome, double> scoreFunction,
List<Func<CPPNNEATActivationFunction>> canonicalFunctionList,
Func<CPPNNEATActivationFunction> outputActivationFunction)
            : base(NUMBER_OF_INPUTS, populationSize, scoreFunction, canonicalFunctionList, outputActivationFunction, true)
        {
        }

        protected override MandelbrotCPPNNEATGenome CreateGenome()
        {
            return new MandelbrotCPPNNEATGenome(this);
        }
    }
}