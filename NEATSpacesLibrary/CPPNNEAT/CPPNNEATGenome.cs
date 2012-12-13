﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGenome : SpeciatedGenome<CPPNNEATGeneCollection, CPPNNetwork>
    {
        private static int SMALL_GENOME_THRESHOLD = 20;

        public class DifferenceAnalysis
        {
            public class DifferenceAnalysisCollection
            {
                public List<CPPNNEATLinkGene> Excess { get; private set; }
                public List<CPPNNEATLinkGene> Disjoint { get; private set; }

                public DifferenceAnalysisCollection()
                {
                    Excess = new List<CPPNNEATLinkGene>();
                    Disjoint = new List<CPPNNEATLinkGene>();
                }
            }

            public class Match
            {
                public CPPNNEATLinkGene FirstCollection {get; private set;}
                public CPPNNEATLinkGene SecondCollection {get; private set;}

                public Match(CPPNNEATLinkGene firstCollection, CPPNNEATLinkGene secondCollection)
                {
                    this.FirstCollection = firstCollection;
                    this.SecondCollection = secondCollection;
                }
            }

            public DifferenceAnalysisCollection FirstCollection { get; private set; }
            public DifferenceAnalysisCollection SecondCollection { get; private set; }

            public List<Match> Matches { get; private set; }

            public DifferenceAnalysis(CPPNNEATGeneCollection geneCollection1, 
                                    CPPNNEATGeneCollection geneCollection2)
            {
                this.FirstCollection = new DifferenceAnalysisCollection();
                this.SecondCollection = new DifferenceAnalysisCollection();

                var link1 = geneCollection1.LinkGenes.GetEnumerator();
                var link2 = geneCollection2.LinkGenes.GetEnumerator();

                Matches = new List<Match>();

                link1.MoveNext();
                link2.MoveNext();

                while (link1.Current != null || link2.Current != null)
                {
                    if (link1.Current == null) 
                    {
                        SecondCollection.Excess.Add(link2.Current);
                        link2.MoveNext();
                    }
                    else if (link2.Current == null)
                    {
                        FirstCollection.Excess.Add(link1.Current);
                        link1.MoveNext();
                    }
                    else if (link1.Current.InnovationNumber == link2.Current.InnovationNumber)
                    {
                        Matches.Add(new Match(link1.Current, link2.Current));

                        link1.MoveNext();
                        link2.MoveNext();
                    } 
                    else if (link2.Current.InnovationNumber > link1.Current.InnovationNumber)
                    {
                        FirstCollection.Disjoint.Add(link1.Current);
                        link1.MoveNext();
                    }
                    else if (link1.Current.InnovationNumber > link2.Current.InnovationNumber)
                    {
                        SecondCollection.Disjoint.Add(link2.Current);
                        link2.MoveNext();
                    }
                }
            }
        }

        public CPPNNEATGenome() 
        {
            this.GeneCollection = new CPPNNEATGeneCollection();
            this.GeneCollection.Parent = this;
        }

        public CPPNNEATGenome(CPPNNEATGenome parent, CPPNNEATGenome partner): this()
        {
            this.Parent = parent.Parent;

            var parentGA = Parent as CPPNNEATGA;
            var differences = new DifferenceAnalysis(parent.GeneCollection, partner.GeneCollection);

            var disjointAndExcessSource = differences.FirstCollection;

            if (partner.Score > parent.Score)
            {
                disjointAndExcessSource = differences.SecondCollection;
            }
            else if (partner.Score == parent.Score)
            {
                disjointAndExcessSource = (parentGA.Random.NextDouble() <= 0.5) ? differences.FirstCollection : 
                                                                    differences.SecondCollection;
            }

            Func<CPPNNEATLinkGene, CPPNNEATLinkGene, double> weightSelector = null;

            if (parentGA.Random.NextDouble() <= parentGA.MateByAveragingRate)
            {
                weightSelector = (first, second) => (first.Weight + second.Weight) / 2;
            }
            else
            {
                weightSelector = (first, second) => (parentGA.Random.NextDouble() <= 0.5)? first.Weight : second.Weight;
            }

            foreach (var match in differences.Matches)
            {
                var geneToCopy = match.FirstCollection;

                var newGene = geneToCopy.Copy();
                newGene.Enabled = true;
                newGene.Weight = weightSelector(match.FirstCollection, match.SecondCollection);

                GeneCollection.TryAddLinkGene(newGene);

                if ((!match.FirstCollection.Enabled || !match.SecondCollection.Enabled) &&
                    Parent.Random.NextDouble() <= parentGA.DisableGeneRate)
                {
                    GeneCollection.DisableLinkGene(newGene.InnovationNumber);
                }
            }

            foreach (var linkGene in disjointAndExcessSource
                                    .Disjoint.Union(disjointAndExcessSource.Excess))
            {
                GeneCollection.TryAddLinkGene(linkGene.Copy());
            }
        }

        public double GetActivation(double[] input)
        {
            return Phenome.GetActivation(input);
        }

        public override double CompatibilityDistance(SpeciatedGenome<CPPNNEATGeneCollection, CPPNNetwork> genome)
        {
            var differences = new DifferenceAnalysis(this.GeneCollection, genome.GeneCollection);

            var totalExcess = differences.FirstCollection.Excess.Count + differences.SecondCollection.Excess.Count;
            var totalDisjoint = differences.FirstCollection.Disjoint.Count + differences.SecondCollection.Disjoint.Count;

            var averageWeightDifference = differences.Matches
                                                    .Select(match => Math.Abs(match.FirstCollection.Weight - match.SecondCollection.Weight))
                                                    .Average();

            var analysis1 = GetFunctionAnalysis();
            var analysis2 = (genome as CPPNNEATGenome).GetFunctionAnalysis();

            var averageFunctionDifference = (Parent as CPPNNEATGA).AllFunctions
                                                    .Select(delegate(Func<double, double> function) {
                                                        return Math.Abs(analysis1[function] - analysis2[function]);
                                                    })
                                                .Average();

            var n = (double)Math.Max(this.GeneCollection.LinkGenes.Count(),
                            genome.GeneCollection.LinkGenes.Count());

            if (n <= SMALL_GENOME_THRESHOLD)
            {
                n = 1;
            }

            var parent = Parent as CPPNNEATGA;

            return parent.ExcessGenesWeight * (totalExcess / n) +
                 parent.DisjointGenesWeight * (totalDisjoint / n) +
                 parent.MatchingGenesWeight * averageWeightDifference +
                 parent.FunctionDifferenceWeight * averageFunctionDifference;
        }

        public Dictionary<Func<double, double>, int> GetFunctionAnalysis()
        {
            var result = new Dictionary<Func<double, double>, int>();

            foreach (var function in (Parent as CPPNNEATGA).AllFunctions)
            {
                result[function] = 0;
            }

            foreach (var neuronGene in GeneCollection.NeuronGenes)
            {
                if (neuronGene.ActivationFunction != null)
                {
                    result[neuronGene.ActivationFunction] += 1;
                }
            }

            return result;
        }

        protected override CPPNNetwork GetPhenome()
        {
            GeneCollection.Update();
            return GeneCollection.Phenome;
        }

        protected override Genome<CPPNNEATGeneCollection, CPPNNetwork>[] InnerCrossover(Genome<CPPNNEATGeneCollection, CPPNNetwork> partner)
        {
            return new Genome<CPPNNEATGeneCollection, CPPNNetwork>[] 
            {
                new CPPNNEATGenome(this, (CPPNNEATGenome)partner),
                new CPPNNEATGenome(this, (CPPNNEATGenome)partner)
            };
        }

        private const int LINK_MUTATION_INDEX = 0;
        private const int NEURON_MUTATION_INDEX = 1;
        private const int WEIGHT_MUTATION_INDEX = 2;
        private const int TOTAL_MUTATIONS = 3;

        protected override void InnerMutate()
        {
            var parent = Parent as CPPNNEATGA;

            var mutationProbabilities = new double[TOTAL_MUTATIONS];

            mutationProbabilities[LINK_MUTATION_INDEX] = parent.NewLinkRate;
            mutationProbabilities[NEURON_MUTATION_INDEX]  = parent.NewNeuronRate;
            mutationProbabilities[WEIGHT_MUTATION_INDEX] = parent.WeightMutationRate;

            var selected = Enumerable.Range(0, TOTAL_MUTATIONS).RouletteWheelSingle(i => mutationProbabilities[i]);

            switch (selected)
            {
                case LINK_MUTATION_INDEX:
                    GeneCollection.TryCreateLinkGene();
                    break;

                case NEURON_MUTATION_INDEX:
                    GeneCollection.TryCreateNeuronGene();
                    break;

                case WEIGHT_MUTATION_INDEX:
                    foreach (var link in GeneCollection.LinkGenes)
                    {
                        if (parent.Random.NextDouble() <= parent.WeightMutationRate)
                        {
                            if (Parent.Random.NextDouble() <= parent.WeightPertubationRate)
                            {
                                link.Weight += (MathExtensions.RandomNumber() * 2 - 1) * parent.MaxPerturbation; 
                            }
                            else
                            {
                                link.Weight = parent.GetRandomWeight();
                            }
                        }
                    }
                    break;
            }
        }

        public override void Initialise()
        {
            GeneCollection.Initialise();
        }

        protected override string InnerDebugInformation()
        {
            return String.Join("-", GeneCollection.LinkGenes.Select(link => link.DebugInformation()));
        }

        public override string ToString() 
        {
            return String.Join("\r\n", GeneCollection.ValidLinks.Select(link => link.ToString()));
        }

        public override Genome<CPPNNEATGeneCollection, CPPNNetwork> InnerCopy()
        {
            var result = (CPPNNEATGenome)this.MemberwiseClone();

            result.GeneCollection = new CPPNNEATGeneCollection();
            result.GeneCollection.Parent = result;

            foreach (var linkGene in GeneCollection.LinkGenes)
            {
                result.GeneCollection.TryAddLinkGene(linkGene.Copy());
            }

            return result;
        }
    }
}
