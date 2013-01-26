using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPPNNEAT.CPPNNEAT;
using CPPNNEAT.GeneticAlgorithms;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CPPNNEAT.Extensions;
using MandelbrotCPPNNEAT;

namespace MandelbrotCPPNNEAT
{

    /// <summary>
    /// Evolves new Mandelbrot sets based on the CPPN-NEAT algorithm.
    /// </summary>
    public class MandelbrotCPPNNEATGA : BaseCPPNNEATGA<MandelbrotCPPNNEATGenome, CPPNNEATGeneCollection,
                                                        MandelbrotCPPNNEATPhenome>, ICPPNNEATGA
    {
        private const int NUMBER_OF_INPUTS = 2;

        /// <summary>
        /// The complex number at the top-left corner of the view.
        /// </summary>
        public Complex ViewPosition
        {
            get;
            set;
        }

        /// <summary>
        /// The distance, in the complex plane, between the two corners of each side of the view.
        /// </summary>
        public double ViewScale
        {
            get;
            set;
        }
        
        /// <summary>
        /// Initialises a new Mandelbrot CPPN-NEAT GA.
        /// </summary>
        /// <param name="populationSize">The number of individuals in the population.</param>
        /// <param name="scoreFunction">The function used to determine the fitness of an individual.</param>
        /// <param name="canonicalFunctionList">The list of function factories used to generate the activation functions 
        /// for the hidden neurons.</param>
        /// <param name="outputActivationFunction">The function factory used to generate the output activation function. </param>
        public MandelbrotCPPNNEATGA(int populationSize, 
                        Func<MandelbrotCPPNNEATGenome, double> scoreFunction,
                        List<Func<CPPNNEATActivationFunction>> canonicalFunctionList, 
                        Func<CPPNNEATActivationFunction> outputActivationFunction)
            : base(NUMBER_OF_INPUTS, populationSize, scoreFunction, canonicalFunctionList, outputActivationFunction, true) 
        {
        }
    }
}
