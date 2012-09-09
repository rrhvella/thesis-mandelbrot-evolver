using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGA : 
        BaseSpeciatedSteadyStateGA<CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork> 
    {

        public IEnumerable<Func<double[], double>> CanonicalFunctionList
        {
            get;
            private set;
        }

        public CPPNNEATGA(int numberOfInputs, int populationSize, Func<CPPNNEATGenome, double> scoreFunction, 
                        List<Func<double[], double>> canonicalFunctionList): base(populationSize, scoreFunction)
        {
            this.NumberOfInputs = numberOfInputs;
            this.CanonicalFunctionList = canonicalFunctionList;
        }

        public int NumberOfInputs
        {
            get;
            private set;
        }

        public double WeightMutationRate
        {
            get;
            set;
        }

        public double NewNeuronRate
        {
            get;
            set;
        }

        public double NewLinkRate
        {
            get;
            set;
        }

        public double WeightPertubationRate
        {
            get;
            set;
        }

        public double MaxPerturbation
        {
            get;
            set;
        }

        public double DisableGeneRate
        {
            get;
            set;
        }
    }
}
