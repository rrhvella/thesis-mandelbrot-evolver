using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.Extensions;
using MandelbrotCPPNNEAT;

namespace MandelbrotCPPNNEAT
{
    /// <summary>
    /// The genome of an individual inside a Mandelbrot CPPN-NEAT GA <seealso cref="MandelbrotCPPNNEATGA"/>
    /// </summary>
    public class MandelbrotCPPNNEATGenome: CPPNNEATGenome<CPPNNEATGeneCollection,
                                                            MandelbrotCPPNNEATPhenome>
    {

        public MandelbrotCPPNNEATGenome(): base()
        {
        }

        /// <summary>
        /// Initialises an individual based on the genomes of its parents.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="partner"></param>
        public MandelbrotCPPNNEATGenome(MandelbrotCPPNNEATGenome parent, MandelbrotCPPNNEATGenome partner)
            :base(parent, partner)
        {
        }

        /// <summary>
        /// Returns the phenome of the individual <seealso cref="MandelbrotCPPNNEATPhenome"/>
        /// </summary>
        /// <returns></returns>
        protected override MandelbrotCPPNNEATPhenome GetPhenome()
        {
            var parent = Parent as MandelbrotCPPNNEATGA;
            return new MandelbrotCPPNNEATPhenome(GetNetwork(), parent.ViewPosition, parent.ViewScale);
        }

        /// <summary>
        /// Returns the child of this genome when it mates with another individual.
        /// </summary>
        /// <param name="partner"></param>
        /// <returns></returns>
        protected override Genome<CPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> 
                InnerCrossover(Genome<CPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> partner)
        {
            return new MandelbrotCPPNNEATGenome((MandelbrotCPPNNEATGenome)this, (MandelbrotCPPNNEATGenome)partner);
        }
    }
}
