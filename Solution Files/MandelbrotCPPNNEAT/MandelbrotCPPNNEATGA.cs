using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NEATSpacesLibrary.Extensions;
using MandelbrotCPPNNEAT;

namespace MandelbrotCPPNNEAT
{

    public class MandelbrotCPPNNEATGA : BaseCPPNNEATGA<MandelbrotCPPNNEATGenome, CPPNNEATGeneCollection,
                                                        MandelbrotCPPNNEATPhenome>, ICPPNNEATGA
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
    }
}
