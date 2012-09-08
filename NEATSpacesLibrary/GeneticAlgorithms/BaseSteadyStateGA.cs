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

        private Random random;

        private GenomeType best;
        private bool bestCacheExpired;
        public GenomeType Best
        {
            get
            {
                if (bestCacheExpired)
                {
                    best = Population.MaxBy(elem => elem.Score);
                }

                return best;
            }
            private set
            {
                best = value;
            }
        }

        public BaseSteadyStateGA(int populationSize)
        {
            this.populationSize = populationSize;
            this.Population = new List<GenomeType>(populationSize);

            this.random = new Random();

            (Enumerable.Range(0, populationSize)).AsParallel().ForAll(new Action<int>(delegate (int i) {
                var newGenome = new GenomeType();
                newGenome.Parent = this;

                lock (Population)
                {
                    Population.Add(newGenome);
                }

                newGenome.Initialise();
                newGenome.Update();

                if (GenomeAdded != null)
                {
                    GenomeAdded(this, new GenomeEventArgs<GenomeType>(newGenome));
                }
            }));


            bestCacheExpired = true;
        }

        public void Iterate()
        {
            if (random.NextDouble() <= CrossoverRate)
            {
                var selection = PerformSelection();
                var children = selection.Parent.Crossover(selection.Partner);

                if (children == null)
                {
                    Enumerable.Range(0, selection.IndividualsToReplace.Length).AsParallel().ForAll(new Action<int>(
                        delegate(int i)
                        {
                            lock (Population)
                            {
                                Population.Remove(selection.IndividualsToReplace[i]);
                            }

                            if (GenomeRemoved != null)
                            {
                                GenomeRemoved(this, new GenomeEventArgs<GenomeType>(selection.IndividualsToReplace[i]));
                            }

                            lock (Population)
                            {
                                Population.Add((GenomeType)children[i]);
                            }

                            children[i].Parent = this;

                            children[i].Mutate();
                            children[i].Update();

                            if (GenomeAdded != null)
                            {
                                GenomeAdded(this, new GenomeEventArgs<GenomeType>((GenomeType)children[i]));
                            }
                        }));

                    bestCacheExpired = true;
                }
            }

            if (IterationComplete != null)
            {
                IterationComplete(this, new EventArgs());
            }
        }

        protected abstract GASelectionResult<GenomeType> PerformSelection();
    }
}
