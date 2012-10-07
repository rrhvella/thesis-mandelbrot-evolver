using System;
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
            var differences = new DifferenceAnalysis(parent.GeneCollection, partner.GeneCollection);

            var disjointAndExcessSource = differences.FirstCollection;

            if (partner.Score > parent.Score)
            {
                disjointAndExcessSource = differences.SecondCollection;
            }
            else if (partner.Score == parent.Score)
            {
                disjointAndExcessSource = (Parent.Random.NextDouble() <= 0.5) ? differences.FirstCollection : 
                                                                    differences.SecondCollection;
            }

            foreach (var match in differences.Matches)
            {
                var geneToCopy = match.FirstCollection;

                if (Parent.Random.NextDouble() <= 0.5)
                {
                    geneToCopy = match.SecondCollection;
                }

                var newGene = geneToCopy.Copy();
                GeneCollection.TryAddLinkGene(newGene);

                if (!geneToCopy.Enabled && Parent.Random.NextDouble() <= (Parent as CPPNNEATGA).EnableGeneRate)
                {
                    GeneCollection.EnableLinkGene(newGene.InnovationNumber);
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

            var averageFunctionDifference = (Parent as CPPNNEATGA).CanonicalFunctionList
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

            foreach (var function in (Parent as CPPNNEATGA).CanonicalFunctionList)
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

        protected override void InnerMutate()
        {
            var parent = Parent as CPPNNEATGA;
            if (parent.Random.NextDouble() <= parent.NewLinkRate)
            {
                 GeneCollection.TryCreateLinkGene();
            }

            if (parent.Random.NextDouble() <= parent.NewNeuronRate)
            {
                 GeneCollection.TryCreateNeuronGene();
            }

            foreach (var link in GeneCollection.LinkGenes)
            {
                if (parent.Random.NextDouble() <= parent.WeightMutationRate)
                {
                    if (Parent.Random.NextDouble() <= parent.WeightPertubationRate)
                    {
                        link.Weight += (Parent.Random.NextDouble() - 0.5) * 2 * parent.MaxPerturbation;
                    }
                    else
                    {
                        link.Weight = parent.GetRandomWeight();
                    }
                }
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
