using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGA :
        BaseSpeciatedGA<CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork>
    {

        private Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int> edgeInnovationNumberMap;
        private Dictionary<int, Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>> edgeMap;
        private Dictionary<int, CPPNNEATNeuronGene> hiddenNeuronMap;

        public IList<Func<double, double>> CanonicalFunctionList
        {
            get;
            private set;
        }

        public CPPNNEATGA(int numberOfInputs, int populationSize, Func<CPPNNEATGenome, double> scoreFunction,
                        List<Func<double, double>> canonicalFunctionList, bool feedForwardOnly): base(populationSize, scoreFunction)
        {
            if (numberOfInputs == 0)
            {
                throw new ApplicationException("Numbers of inputs cannot be 0.");
            }

            this.NumberOfInputs = numberOfInputs;
            this.CanonicalFunctionList = canonicalFunctionList;

            this.edgeInnovationNumberMap = new Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int>();
            this.hiddenNeuronMap = new Dictionary<int, CPPNNEATNeuronGene>();
            this.edgeMap = new Dictionary<int, Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>>();

            this.defaultNeuronGenes = new List<CPPNNEATNeuronGene>();
            this.defaultLinkGenes = new List<CPPNNEATLinkGene>();

            var outputGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 1, CPPNNeuronType.Output, 
                                                    canonicalFunctionList.RandomSingle());

            var currentGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 0, CPPNNeuronType.Bias, null);
            defaultNeuronGenes.Add(currentGene);
            defaultLinkGenes.Add(new CPPNNEATLinkGene(GetEdgeInnovationNumber(currentGene, outputGene), currentGene, outputGene, 0));

            foreach (var i in Enumerable.Range(0, numberOfInputs))
            {
                currentGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 0, CPPNNeuronType.Input, null);

                defaultNeuronGenes.Add(currentGene);
                defaultLinkGenes.Add(new CPPNNEATLinkGene(GetEdgeInnovationNumber(currentGene, outputGene), currentGene, outputGene, 0));
            }

            this.FeedForwardOnly = feedForwardOnly;

            defaultNeuronGenes.Add(outputGene);
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

        public double MateByAveragingRate
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
        
        private List<CPPNNEATNeuronGene> defaultNeuronGenes;
        public IList<CPPNNEATNeuronGene> DefaultNeuronGenes
        {
            get
            {
                return defaultNeuronGenes.AsReadOnly();
            }
            private set
            {
                defaultNeuronGenes = (List<CPPNNEATNeuronGene>)value;
            }
        }

        private List<CPPNNEATLinkGene> defaultLinkGenes;
        public IList<CPPNNEATLinkGene> DefaultLinkGenes
        {
            get
            {
                return defaultLinkGenes.AsReadOnly();
            }
            private set
            {
                defaultLinkGenes = (List<CPPNNEATLinkGene>)value;
            }
        }

        public double GetRandomWeight()
        {
            return (Random.NextDouble() - 0.5) * 2 * MaxWeight;
        }


        private int edgeInnovationNumber = 0;
        private int neuronInnovationNumber = 0;
        public bool FeedForwardOnly
        {
            get;
            private set;
        }

        public int GetEdgeInnovationNumber(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            var key = new Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>(from, to);

            if (!edgeInnovationNumberMap.ContainsKey(key))
            {
                edgeInnovationNumberMap[key] = edgeInnovationNumber;
                edgeMap[edgeInnovationNumber] = key;

                edgeInnovationNumber++;
            }

            return edgeInnovationNumberMap[key];
        }

        public CPPNNEATNeuronGene GetHiddenNeuron(int innovationNumber)
        {
            if (!hiddenNeuronMap.ContainsKey(innovationNumber))
            {
                var edge = edgeMap[innovationNumber];
                var level = (edge.Item1.Level + edge.Item2.Level) / 2;

                hiddenNeuronMap[innovationNumber] = new CPPNNEATNeuronGene(neuronInnovationNumber++, level, CPPNNeuronType.Hidden, 
                                                                    CanonicalFunctionList.RandomSingle());
            }

            return hiddenNeuronMap[innovationNumber];
        }
    }
}
