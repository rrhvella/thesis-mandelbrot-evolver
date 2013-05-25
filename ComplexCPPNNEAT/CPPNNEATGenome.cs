using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotNetExtensions;
using GeneticAlgorithms;

namespace ComplexCPPNNEAT
{
    public abstract class CPPNNEATGenome<PType> : SpeciatedGenome<CPPNNEATGeneCollection, PType>, ICPPNNEATGenome
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
                public CPPNNEATLinkGene FirstCollection { get; private set; }

                public CPPNNEATLinkGene SecondCollection { get; private set; }

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

        public CPPNNEATGenome(ICPPNNEATGA parent): base(parent)
        {
            this.GeneCollection = new CPPNNEATGeneCollection(this);
        }

        public CPPNNEATGenome(ICPPNNEATGA parentGA, CPPNNEATGenome<PType> parent, CPPNNEATGenome<PType> partner)
            : this(parentGA)
        {
            var differences = new DifferenceAnalysis(parent.GeneCollection, partner.GeneCollection);

            var disjointAndExcessSource = differences.FirstCollection;

            if (partner.Score > parent.Score)
            {
                disjointAndExcessSource = differences.SecondCollection;
            }
            else if (partner.Score == parent.Score)
            {
                disjointAndExcessSource = (MathExtensions.RandomNumber() <= 0.5) ? differences.FirstCollection :
                                                                    differences.SecondCollection;
            }

            Func<CPPNNEATLinkGene, CPPNNEATLinkGene, Complex> weightSelector = null;

            if (MathExtensions.RandomNumber() <= parentGA.MateByAveragingRate)
            {
                weightSelector = (first, second) => (first.Weight + second.Weight) / 2;
            }
            else
            {
                weightSelector = (first, second) => (MathExtensions.RandomNumber() <= 0.5) ? first.Weight : second.Weight;
            }

            foreach (var match in differences.Matches)
            {
                var geneToCopy = match.FirstCollection;

                var newGene = geneToCopy.Copy();
                newGene.Enabled = true;
                newGene.Weight = weightSelector(match.FirstCollection, match.SecondCollection);

                GeneCollection.TryAddLinkGene(newGene);

                if ((!match.FirstCollection.Enabled || !match.SecondCollection.Enabled) &&
                    MathExtensions.RandomNumber() <= parentGA.DisableGeneRate)
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

        public override double CompatibilityDistance(SpeciatedGenome<CPPNNEATGeneCollection, PType> genome)
        {
            var differences = new DifferenceAnalysis(this.GeneCollection, genome.GeneCollection);

            var totalExcess = differences.FirstCollection.Excess.Count + differences.SecondCollection.Excess.Count;
            var totalDisjoint = differences.FirstCollection.Disjoint.Count + differences.SecondCollection.Disjoint.Count;

            var averageWeightDifference = differences.Matches
                                                    .Select(match =>
                                                            (match.FirstCollection.Weight - match.SecondCollection.Weight)
                                                            .Magnitude)
                                                    .Average();

            var n = (double)Math.Max(this.GeneCollection.LinkGenes.Count(),
                            genome.GeneCollection.LinkGenes.Count());

            if (n <= SMALL_GENOME_THRESHOLD)
            {
                n = 1;
            }

            var parent = Parent as ICPPNNEATGA;
            var averageFunctionDifference = GetAverageFunctionDifference(this, (CPPNNEATGenome<PType>)genome);

            return parent.ExcessGenesWeight * (totalExcess / n) +
                 parent.DisjointGenesWeight * (totalDisjoint / n) +
                 parent.MatchingGenesWeight * averageWeightDifference +
                 parent.FunctionDifferenceWeight * averageFunctionDifference;
        }

        private static double GetAverageFunctionDifference(CPPNNEATGenome<PType> genome1,
        CPPNNEATGenome<PType> genome2)
        {
            var functionDifferenceMap = genome1.GeneCollection.ActivationFunctions.GroupBy(func => func.Label)
                                                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var activationFunctionGroup in genome2.GeneCollection.ActivationFunctions.GroupBy(func => func.Label))
            {
                if (!functionDifferenceMap.ContainsKey(activationFunctionGroup.Key))
                {
                    functionDifferenceMap[activationFunctionGroup.Key] = 0;
                }

                functionDifferenceMap[activationFunctionGroup.Key] =
                                    Math.Abs(functionDifferenceMap[activationFunctionGroup.Key] -
                                        activationFunctionGroup.Count());
            }

            return functionDifferenceMap.Average(pair => pair.Value);
        }

        protected CPPNNetwork GetNetwork()
        {
            GeneCollection.Update();
            return GeneCollection.Phenome;
        }

        private const int LINK_MUTATION_INDEX = 0;
        private const int NEURON_MUTATION_INDEX = 1;
        private const int WEIGHT_MUTATION_INDEX = 2;
        private const int TOTAL_MUTATIONS = 3;

        protected override void InnerMutate()
        {
            var parent = Parent as ICPPNNEATGA;

            var mutationProbabilities = new double[TOTAL_MUTATIONS];

            mutationProbabilities[LINK_MUTATION_INDEX] = parent.NewLinkRate;
            mutationProbabilities[NEURON_MUTATION_INDEX] = parent.NewNeuronRate;
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
                        if (MathExtensions.RandomNumber() <= parent.WeightMutationRate)
                        {
                            if (MathExtensions.RandomNumber() <= parent.WeightPertubationRate)
                            {
                                link.Weight += MathExtensions.ComplexRandom(-parent.MaxPerturbation,
                                                                            parent.MaxPerturbation);
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

        public override string ToString()
        {
            return String.Join("\r\n", GeneCollection.ValidLinks.Select(link => link.ToString()));
        }

        public override Genome<CPPNNEATGeneCollection, PType> InnerCopy()
        {
            var result = (CPPNNEATGenome<PType>)this.MemberwiseClone();

            result.GeneCollection = new CPPNNEATGeneCollection(result);

            foreach (var linkGene in GeneCollection.LinkGenes)
            {
                result.GeneCollection.TryAddLinkGene(linkGene.Copy());
            }

            return result;
        }
    }
}