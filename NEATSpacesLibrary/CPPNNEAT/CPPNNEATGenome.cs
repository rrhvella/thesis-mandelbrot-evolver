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

        private Random random;

        public CPPNNEATGenome() 
        {
            this.random = new Random();
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
                disjointAndExcessSource = (random.NextDouble() <= 0.5) ? differences.FirstCollection : 
                                                                    differences.SecondCollection;
            }

            foreach (var match in differences.Matches)
            {
                var geneToCopy = match.FirstCollection;

                if (random.NextDouble() <= 0.5)
                {
                    geneToCopy = match.SecondCollection;
                }

                var newGene = geneToCopy.Copy();
                GeneCollection.TryAddLinkGene(newGene);

                if (geneToCopy.Enabled && (!match.FirstCollection.Enabled || !match.SecondCollection.Enabled) &&
                    random.NextDouble() <= (Parent as CPPNNEATGA).DisableGeneRate)
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

            var averageFunctionDifference = (Parent as CPPNNEATGA).CanonicalFunctionList
                                                    .Select(delegate(Func<double, double> function) {
                                                        return Math.Abs(analysis1[function] - analysis2[function]);
                                                    })
                                                .Average();

            var n = (double)Math.Max(this.GeneCollection.LinkGenes.Count(),
                            genome.GeneCollection.LinkGenes.Count());

            var parent = Parent as CPPNNEATGA;

            return parent.ExcessGenesWeight * (totalExcess / n) +
                 parent.DisjointGenesWeight * (totalDisjoint / n) +
                 parent.MatchingGenesWeight * averageWeightDifference +
                 parent.FunctionDifferenceWeight * averageFunctionDifference;
        }

        public Dictionary<Func<double, double>, int> GetFunctionAnalysis()
        {
            var result = (Parent as CPPNNEATGA).CanonicalFunctionList.ToDictionary(elem => elem, elem => 0);

            var groupedActivationFunctions = (from neuronGene in GeneCollection.NeuronGenes
                                             where neuronGene.ActivationFunction != null
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
            var probabilityMap = new[] { parent.WeightMutationRate, parent.NewLinkRate, 
                                        parent.NewNeuronRate, parent.NoChangeRate };

            var selection = Enumerable.Range(0, probabilityMap.Length).RouletteWheelSingle(i => probabilityMap[i]);
            var performWeightMutation = false;

            switch (selection)
            {
                case 0:
                    performWeightMutation = true;
                    break;

                case 1:
                    if (!GeneCollection.TryCreateLinkGene())
                    {
                        performWeightMutation = true;
                    }

                    break;

                case 2:
                    if (!GeneCollection.TryCreateNeuronGene())
                    {
                        performWeightMutation = true;
                    }

                    break;
            }

            if (performWeightMutation)
            {
                foreach (var link in GeneCollection.LinkGenes)
                {
                    if (random.NextDouble() <= parent.WeightPertubationRate)
                    {
                        link.Weight += (random.NextDouble() - 0.5) * 2 * parent.MaxPerturbation;
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
    }
}
