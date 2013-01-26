using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPPNNEAT.Extensions;
using CPPNNEAT.GeneticAlgorithms;
using System.Numerics;

namespace CPPNNEAT.CPPNNEAT
{
    /// <summary>
    /// CPPN-NEAT genome.
    /// </summary>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public abstract class CPPNNEATGenome<GType, PType> : SpeciatedGenome<GType, PType>, ICPPNNEATGenome
        where GType: CPPNNEATGeneCollection, new()
    {
        /// <summary>
        /// The number of links below which a genome is considered small.
        /// </summary>
        private static int SMALL_GENOME_THRESHOLD = 20;

        /// <summary>
        /// Represents an analysis of the differences between two genomes.
        /// </summary>
        public class DifferenceAnalysis
        {
            /// <summary>
            /// Represents a collection of results for a single genome.
            /// </summary>
            public class DifferenceAnalysisCollection
            {
                /// <summary>
                /// The genes present in this genome which come after the last gene in the other 
                /// genome.
                /// </summary>
                public List<CPPNNEATLinkGene> Excess { get; private set; }
                /// <summary>
                /// The genes present in this genome which are disjoint from the other genome.
                /// </summary>
                public List<CPPNNEATLinkGene> Disjoint { get; private set; }

                public DifferenceAnalysisCollection()
                {
                    Excess = new List<CPPNNEATLinkGene>();
                    Disjoint = new List<CPPNNEATLinkGene>();
                }
            }

            /// <summary>
            /// Represents a link gene which is present in both genomes.
            /// </summary>
            public class Match
            {
                /// <summary>
                /// The instance in the first genome.
                /// </summary>
                public CPPNNEATLinkGene FirstCollection {get; private set;}

                /// <summary>
                /// The instance in the second genome.
                /// </summary>
                public CPPNNEATLinkGene SecondCollection {get; private set;}

                /// <summary>
                /// </summary>
                /// <param name="firstCollection">The instance in the first genome.</param>
                /// <param name="secondCollection">The instance in the second genome.</param>
                public Match(CPPNNEATLinkGene firstCollection, CPPNNEATLinkGene secondCollection)
                {
                    this.FirstCollection = firstCollection;
                    this.SecondCollection = secondCollection;
                }
            }

            /// <summary>
            /// The disjoint and excess genes in the first genome.
            /// </summary>
            public DifferenceAnalysisCollection FirstCollection { get; private set; }
            /// <summary>
            /// The disjoint and excess genes in the second genome.
            /// </summary>
            public DifferenceAnalysisCollection SecondCollection { get; private set; }

            /// <summary>
            /// The common genes between the two genomes.
            /// </summary>
            public List<Match> Matches { get; private set; }

            /// <summary>
            /// </summary>
            /// <param name="geneCollection1">The sequence of the first genome.</param>
            /// <param name="geneCollection2">The sequence of the second genome. </param>
            public DifferenceAnalysis(CPPNNEATGeneCollection geneCollection1, 
                                    CPPNNEATGeneCollection geneCollection2)
            {
                this.FirstCollection = new DifferenceAnalysisCollection();
                this.SecondCollection = new DifferenceAnalysisCollection();

                //Place the genes of the two sequences side by side, iterate through them, and
                //seperate the matched, disjoint and excess genes.
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
            this.GeneCollection = new GType();
            this.GeneCollection.Parent = this;
        }

        /// <summary>
        /// Initialises an individual based on the genomes of its parents.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="partner"></param>
        public CPPNNEATGenome(CPPNNEATGenome<GType, PType> parent, CPPNNEATGenome<GType, PType>  partner): this()
        {
            this.Parent = parent.Parent;

            var parentGA = Parent as ICPPNNEATGA;
            var differences = new DifferenceAnalysis(parent.GeneCollection, partner.GeneCollection);

            //Pick the source of the excess and disjoint genes based on the fitness.
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

            //Determine the method to combine the weights.
            Func<CPPNNEATLinkGene, CPPNNEATLinkGene, Complex> weightSelector = null;

            if (parentGA.Random.NextDouble() <= parentGA.MateByAveragingRate)
            {
                weightSelector = (first, second) => (first.Weight + second.Weight) / 2;
            }
            else
            {
                weightSelector = (first, second) => (parentGA.Random.NextDouble() <= 0.5)? first.Weight : second.Weight;
            }

            //For each matched genes:
            //- Copy the matched gene into the child.
            //- Combine the weights.
            //- Disable the gene, if it is disabled in either parent.
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

            //Copy the disjoint and excess genes from the selected source.
            foreach (var linkGene in disjointAndExcessSource
                                    .Disjoint.Union(disjointAndExcessSource.Excess))
            {
                GeneCollection.TryAddLinkGene(linkGene.Copy());
            }
        }

        public override double CompatibilityDistance(SpeciatedGenome<GType, PType> genome)
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
            var averageFunctionDifference = GetAverageFunctionDifference(this, (CPPNNEATGenome<GType, PType>)genome);

            return parent.ExcessGenesWeight * (totalExcess / n) +
                 parent.DisjointGenesWeight * (totalDisjoint / n) +
                 parent.MatchingGenesWeight * averageWeightDifference +
                 parent.FunctionDifferenceWeight * averageFunctionDifference;
        }

        /// <summary>
        /// Returns the average difference between the function counts of genome1 and genome2.
        /// </summary>
        /// <param name="genome1"></param>
        /// <param name="genome2"></param>
        /// <returns></returns>
        private static double GetAverageFunctionDifference(CPPNNEATGenome<GType, PType> genome1, 
                                                        CPPNNEATGenome<GType, PType> genome2)
        {
            var functionDifferenceMap = genome1.GeneCollection.ActivationFunctions.GroupBy(func => func.Label)
                                                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var activationFunctionGroup in genome2.GeneCollection.ActivationFunctions.GroupBy(func => func.Label))
            {
                if(!functionDifferenceMap.ContainsKey(activationFunctionGroup.Key)) 
                {
                    functionDifferenceMap[activationFunctionGroup.Key] = 0;
                }

                functionDifferenceMap[activationFunctionGroup.Key] = 
                                    Math.Abs(functionDifferenceMap[activationFunctionGroup.Key] - 
                                        activationFunctionGroup.Count());
            }
            
            return functionDifferenceMap.Average(pair => pair.Value);
        }

        /// <summary>
        /// Returns the network derived from this genome.
        /// </summary>
        /// <returns></returns>
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

        public override Genome<GType, PType> InnerCopy()
        {
            var result = (CPPNNEATGenome<GType, PType>)this.MemberwiseClone();

            result.GeneCollection = new GType();
            result.GeneCollection.Parent = result;

            foreach (var linkGene in GeneCollection.LinkGenes)
            {
                result.GeneCollection.TryAddLinkGene(linkGene.Copy());
            }

            return result;
        }
    }
}
