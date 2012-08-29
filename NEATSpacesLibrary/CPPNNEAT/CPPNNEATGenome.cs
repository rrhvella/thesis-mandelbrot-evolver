using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public enum NeuronType
    {
        Input,
        Output,
        Hidden
    }

    public abstract class CPPNNEATGenome: Genome<CPPNNEATGene, CPPNNetwork>
    {
        public double GetActivation(double[] input)
        {
            return Phenotype.GetActivation(input);
        }
    }
}
