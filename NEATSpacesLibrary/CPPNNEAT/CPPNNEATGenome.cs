using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGenome : SpeciatedGenome<CPPNNEATGeneCollection, CPPNNetwork>
    {
        private class DifferenceAnalysis
        {
            public class DifferenceAnalysisCollection
            {
                public List<CPPNNEATLinkGene> Excess { get; private set; }
                public List<CPPNNEATLinkGene> Disjoint { get; private set; }
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

                while (link1.Current == null && link2.Current == null)
                {
                    if (link1.Current.InnovationNumber == link2.Current.InnovationNumber)
                    {
                        Matches.Add(new Match(link1.Current, link2.Current));

                        link1.MoveNext();
                        link2.MoveNext();
                    } 
                    else if (link1.Current == null) 
                    {
                        SecondCollection.Excess.Add(link2.Current);
                        link2.MoveNext();
                    }
                    else if (link2.Current == null)
                    {
                        FirstCollection.Excess.Add(link1.Current);
                        link1.MoveNext();
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

        public const int EXCESS_GENES_WEIGHT = 1;
        public const int DISJOINT_GENES_WEIGHT = 1;
        public const int MATCHING_GENES_WEIGHT = 1;
        public const int FUNCTION_DIFF_WEIGHT = 1;

        private Random random;

        public new CPPNNEATGA Parent
        {
            get
            {
                return (CPPNNEATGA)base.Parent;
            }
            set
            {
                GeneCollection.Parent = value;
                base.Parent = value;
            }
        }

        public CPPNNEATGenome() 
        {
            this.random = new Random();
            this.GeneCollection = new CPPNNEATGeneCollection();
        }

        public CPPNNEATGenome(CPPNNEATGenome parent, CPPNNEATGenome partner) 
        {
            var differences = new DifferenceAnalysis(parent.GeneCollection, partner.GeneCollection);

            var disjointAndExcessSource = differences.FirstCollection;

            if (partner.Score > parent.Score)
            {
                disjointAndExcessSource = differences.SecondCollection;
            }
            else if (partner.Score == parent.Score)
            {
                disjointAndExcessSource = null;
            }

            foreach (var match in differences.Matches)
            {
                var newGene = match.FirstCollection;

                if (random.NextDouble() <= 0.5)
                {
                    newGene = match.SecondCollection;
                }

                if (newGene.Enabled && (!match.FirstCollection.Enabled || !match.SecondCollection.Enabled) && 
                        random.NextDouble() <= (Parent as CPPNNEATGA).DisableGeneRate)
                {
                    newGene.Enabled = false;
                }

                GeneCollection.AddLinkGene(newGene);
            }

            if (disjointAndExcessSource != null)
            {
                foreach (var linkGene in disjointAndExcessSource
                                        .Disjoint.Union(disjointAndExcessSource.Excess))
                {
                    GeneCollection.AddLinkGene(linkGene);
                }
            }
            else
            {
                var allDisjointAndExcess = differences.FirstCollection.Disjoint
                                            .Union(differences.FirstCollection.Excess)
                                            .Union(differences.SecondCollection.Disjoint)
                                            .Union(differences.SecondCollection.Excess);

                foreach (var linkGene in allDisjointAndExcess)
                {
                    if(random.NextDouble() <= 0.5) 
                    {
                        GeneCollection.AddLinkGene(linkGene);
                    }
                }
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

            var analysis1 = GetFunctionAnalysis(this.GeneCollection.NeuronGenes);
            var analysis2 = GetFunctionAnalysis(genome.GeneCollection.NeuronGenes);

            var averageFunctionDifference = (Parent as CPPNNEATGA).CanonicalFunctionList
                                                    .Select(delegate(Func<double, double> function) {
                                                        return Math.Abs(analysis1[function] - analysis2[function]);
                                                    })
                                                .Average();

            var n = Math.Max(this.GeneCollection.LinkGenes.Count(),
                            genome.GeneCollection.LinkGenes.Count());

            return EXCESS_GENES_WEIGHT * (totalExcess / n) +
                 DISJOINT_GENES_WEIGHT * (totalDisjoint / n) +
                 MATCHING_GENES_WEIGHT * averageWeightDifference +
                 FUNCTION_DIFF_WEIGHT * averageFunctionDifference;
        }

        private Dictionary<Func<double, double>, int> GetFunctionAnalysis(IEnumerable<CPPNNEATNeuronGene> neuronGenes)
        {
            var result = (Parent as CPPNNEATGA).CanonicalFunctionList.ToDictionary(elem => elem, elem => 0);

            var groupedActivationFunctions = (from neuronGene in neuronGenes
                                             group neuronGene by neuronGene.ActivationFunction into neuronsByActivationFunction
                                             select new
                                             {
                                                 ActivationFunction = neuronsByActivationFunction.Key,
                                                 Count = neuronsByActivationFunction.Count()
                                             });

            foreach (var groupedActivationFunction in groupedActivationFunctions)
            {
                result[groupedActivationFunction.ActivationFunction] = groupedActivationFunction.Count;
            }

            return result;
        }

        protected override CPPNNetwork GetPhenome()
        {
            GeneCollection.Update();
            return GeneCollection.Phenome;
        }

        public override Genome<CPPNNEATGeneCollection, CPPNNetwork>[] Crossover(Genome<CPPNNEATGeneCollection, CPPNNetwork> partner)
        {
            return new Genome<CPPNNEATGeneCollection, CPPNNetwork>[] {
                new CPPNNEATGenome(this, (CPPNNEATGenome)partner),
                new CPPNNEATGenome(this, (CPPNNEATGenome)partner)
            };
        }

        public override void Mutate()
        {
            var parent = Parent as CPPNNEATGA;

            if (random.NextDouble() <= parent.NewLinkRate)
            {
                var numberOfNeuronGenes = GeneCollection.NeuronGenes.Count();
                GeneCollection.CreateLinkGene(random.Next(0, numberOfNeuronGenes), random.Next(1 + parent.NumberOfInputs, numberOfNeuronGenes));
            }

            if (random.NextDouble() <= parent.NewNeuronRate)
            {
                GeneCollection.CreateNeuronGene(random.Next(GeneCollection.LinkGenes.Count()));
            }

            if (random.NextDouble() <= parent.WeightMutationRate)
            {
                foreach (var link in GeneCollection.LinkGenes)
                {
                    if (random.NextDouble() <= parent.WeightPertubationRate)
                    {
                        link.Weight = (random.NextDouble() - 0.5) * 2 * parent.MaxPerturbation;
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
    }
}
