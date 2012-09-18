using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using System.Threading.Tasks;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class GenomeEventArgs<GenomeType> : EventArgs
    {
        public GenomeType Genome 
        {
            get;
            private set;
        }

        public GenomeEventArgs(GenomeType genome) 
        {
            this.Genome = genome;
        }
    }

    public class GASelectionResult<GenomeType>
    {
        public GenomeType Parent
        {
            get;
            private set;
        }

        public GenomeType Partner
        {
            get;
            private set;
        }

        public GenomeType[] IndividualsToReplace
        {
            get;
            private set;
        }

        public GASelectionResult(GenomeType parent, GenomeType partner, 
                        GenomeType individualToReplace1, GenomeType individualToReplace2)
        {
            this.Parent = parent;
            this.Partner = partner;

            this.IndividualsToReplace = new[] { individualToReplace1, individualToReplace2 };
        }
    }

    public abstract class BaseSteadyStateGA<GenomeType, GType, PType> : IGA 
        where GenomeType : Genome<GType, PType>, new()
    {
        public const int DEFAULT_TOURNAMENT_SIZE = 7;

        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeAdded;
        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeRemoved;
        public event EventHandler<EventArgs> IterationComplete;

        private int populationSize;
        public List<GenomeType> Population
        {
            get;
            private set;
        }

        public double CrossoverRate { get; set; }
        public double MutationRate { get; set; }

        protected Random random;

        private GenomeType best;
        private bool bestCacheExpired;
        private Func<GenomeType, double> scoreFunction;
        public GenomeType Best
        {
            get
            {
                if (bestCacheExpired)
                {
                    var bestScore = 0.0;

                    foreach (var genome in Population)
                    {
                        if (genome.PhenomeExpired)
                        {
                            genome.UpdatePhenome();
                            genome.Score = scoreFunction(genome);
                        }

                        if (genome.Score > bestScore)
                        {
                            bestScore = genome.Score;
                            best = genome;
                        }
                    }
                }

                return best;
            }
            private set
            {
                best = value;
            }
        }

        public BaseSteadyStateGA(int populationSize, Func<GenomeType, double> scoreFunction)
        {
            this.populationSize = populationSize;
            this.scoreFunction = scoreFunction;

            this.random = new Random();
        }

        public void Initialise()
        {
            this.Population = new List<GenomeType>(populationSize);

            foreach(var i in Enumerable.Range(0, populationSize)) 
            {
                var newGenome = new GenomeType();
                newGenome.Parent = this;

                Population.Add(newGenome);

                newGenome.Initialise();
                newGenome.PhenomeExpired = true;

                if (GenomeAdded != null)
                {
                    GenomeAdded(this, new GenomeEventArgs<GenomeType>(newGenome));
                }
            }

            Update();
        }

        public void Iterate()
        {
            var selection = PerformSelection();

            if (random.NextDouble() <= CrossoverRate)
            {
                var children = selection.Parent.Crossover(selection.Partner);

                foreach (var i in Enumerable.Range(0, selection.IndividualsToReplace.Length))
                {
                    Population.Remove(selection.IndividualsToReplace[i]);

                    if (GenomeRemoved != null)
                    {
                        GenomeRemoved(this, new GenomeEventArgs<GenomeType>(selection.IndividualsToReplace[i]));
                    }

                    Population.Add((GenomeType)children[i]);

                    children[i].Parent = this;
                    children[i].Mutate();

                    if (GenomeAdded != null)
                    {
                        GenomeAdded(this, new GenomeEventArgs<GenomeType>((GenomeType)children[i]));
                    }
                }
            }

            Update();

            if (IterationComplete != null)
            {
                IterationComplete(this, new EventArgs());
            }
        }

        public void Update()
        {
            bestCacheExpired = true;
        }

        protected abstract GASelectionResult<GenomeType> PerformSelection();
    }
}
