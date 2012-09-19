using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGA :
        BaseSpeciatedSteadyStateGA<CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork>
    {

        private Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int> innovationNumberMap;
        private Dictionary<int, CPPNNEATNeuronGene> hiddenNeuronMap;

        public IList<Func<double, double>> CanonicalFunctionList
        {
            get;
            private set;
        }

        public CPPNNEATGA(int numberOfInputs, int populationSize, Func<CPPNNEATGenome, double> scoreFunction,
                        List<Func<double, double>> canonicalFunctionList): base(populationSize, scoreFunction)
        {
            if (numberOfInputs == 0)
            {
                throw new ApplicationException("Numbers of inputs cannot be 0.");
            }

            this.NumberOfInputs = numberOfInputs;
            this.CanonicalFunctionList = canonicalFunctionList;

            this.innovationNumberMap = new Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int>();
            this.hiddenNeuronMap = new Dictionary<int, CPPNNEATNeuronGene>();

            this.DefaultNeuronGenes = new List<CPPNNEATNeuronGene>();
            this.DefaultLinkGenes = new List<CPPNNEATLinkGene>();

            this.random = new Random();

            var outputGene = new CPPNNEATNeuronGene(CPPNNeuronType.Output, canonicalFunctionList.RandomSingle());

            var currentGene = new CPPNNEATNeuronGene(CPPNNeuronType.Bias, null);
            DefaultNeuronGenes.Add(currentGene);
            DefaultLinkGenes.Add(new CPPNNEATLinkGene(GetInnovationNumber(currentGene, outputGene), currentGene, outputGene, 0));

            foreach (var i in Enumerable.Range(0, numberOfInputs))
            {
                currentGene = new CPPNNEATNeuronGene(CPPNNeuronType.Input, null);

                DefaultNeuronGenes.Add(currentGene);
                DefaultLinkGenes.Add(new CPPNNEATLinkGene(GetInnovationNumber(currentGene, outputGene), currentGene, outputGene, 0));
            }

            DefaultNeuronGenes.Add(outputGene);
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

        public double NoChangeRate
        {
            get;
            set;
        }

        public double DisableGeneRate
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

        public double MaxWeight
        {
            get;
            set;
        }

        public double ExcessGenesWeight
        {
            get;
            set;
        }

        public double DisjointGenesWeight
        {
            get;
            set;
        }

        public double MatchingGenesWeight
        {
            get;
            set;
        }

        public double FunctionDifferenceWeight
        {
            get;
            set;
        }

        public IList<CPPNNEATNeuronGene> DefaultNeuronGenes
        {
            get;
            private set;
        }

        public IList<CPPNNEATLinkGene> DefaultLinkGenes
        {
            get;
            private set;
        }

        public double GetRandomWeight()
        {
            return (random.NextDouble() - 0.5) * 2 * MaxWeight;
        }


        private int innovationNumber = 0;
        public int GetInnovationNumber(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            var key = new Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>(from, to);

            if (!innovationNumberMap.ContainsKey(key))
            {
                innovationNumberMap[key] = innovationNumber++;
            }

            return innovationNumberMap[key];
        }

        public CPPNNEATNeuronGene GetHiddenNeuron(int innovationNumber)
        {
            if (!hiddenNeuronMap.ContainsKey(innovationNumber))
            {
                hiddenNeuronMap[innovationNumber] = new CPPNNEATNeuronGene(CPPNNeuronType.Hidden, 
                                                                    CanonicalFunctionList.RandomSingle());
            }

            return hiddenNeuronMap[innovationNumber];
        }
    }
}
