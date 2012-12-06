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

    public class MandelbrotCPPNNEATGA : BaseCPPNNEATGA<MandelbrotCPPNNEATGenome, MandelbrotCPPNNEATGeneCollection,
                                                        MandelbrotCPPNNEATPhenome>, ICPPNNEATGA
    {
        public Complex InitialPositionOrigin
        {
            get;
            set;
        }

        public double InitialPositionMaxDisplacement
        {
            get;
            set;
        }

        public double DisplacementStandardDeviation
        {
            get;
            set;
        }

        public double ScaleTweakCoefficient
        {
            get;
            set;
        }

        public double ExchangeProbability
        {
            get;
            set;
        }

        public double MinInitialScale
        {
            get;
            set;
        }

        public double MaxInitialScale
        {
            get;
            set;
        }

        public double ViewPositionDistanceCoefficient
        {
            get;
            set;
        }

        public double ViewScaleDistanceCoefficient
        {
            get;
            set;
        }

        public MandelbrotCPPNNEATGA(int numberOfInputs, int populationSize, 
                        Func<MandelbrotCPPNNEATGenome, double> scoreFunction,
                        List<Func<CPPNNEATActivationFunction>> canonicalFunctionList, 
                        Func<CPPNNEATActivationFunction> outputActivationFunction)
            : base(numberOfInputs, populationSize, scoreFunction, canonicalFunctionList, outputActivationFunction, true) 
        {
        }
    }
}
