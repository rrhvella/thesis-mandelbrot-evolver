using System;
using System.Collections.Generic;
using System.Linq;
using DotNetExtensions;

namespace GeneticAlgorithms
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

    public class IterationEventArgs : EventArgs
    {
        public int IterationNumber
        {
            get;
            private set;
        }

        public IterationEventArgs(int iterationNumber)
        {
            this.IterationNumber = iterationNumber;
        }
    }

    public class GAGenerationalSelectionResult<GenomeType, GType, PType>
where GenomeType : Genome<GType, PType>
    {
        public IList<GenomeType> ToRetain
        {
            get;
            private set;
        }

        public IList<GenomeType> ToMutate
        {
            get;
            private set;
        }

        public IList<Tuple<GenomeType, GenomeType>> ParentPairs
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                return ToMutate.Count + ToRetain.Count + ParentPairs.Count;
            }
        }

        public GAGenerationalSelectionResult()
        {
            ToRetain = new List<GenomeType>();
            ToMutate = new List<GenomeType>();
            ParentPairs = new List<Tuple<GenomeType, GenomeType>>();
        }
    }

    public abstract class BaseGA<GenomeType, GType, PType> : IGA
where GenomeType : Genome<GType, PType>
    {
        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeAdded;

        public event EventHandler<EventArgs> SelectionComplete;

        public event EventHandler<IterationEventArgs> IterationBegin;

        public event EventHandler<IterationEventArgs> IterationComplete;

        private int populationSize;

        private List<GenomeType> population;

        public IList<GenomeType> Population
        {
            get
            {
                return population.AsReadOnly();
            }
            private set
            {
                population = (List<GenomeType>)value;
            }
        }

        public virtual bool Failed
        {
            get
            {
                return false;
            }
        }

        public Random Random
        {
            get;
            private set;
        }

        private Func<GenomeType, double> scoreFunction;

        private GenomeType best;
        private bool bestCacheInvalidated;

        public GenomeType Best
        {
            get
            {
                if (bestCacheInvalidated)
                {
                    UpdateGenomes();
                    best = Population.MaxBy(genome => genome.Score);

                    bestCacheInvalidated = false;
                }

                return best;
            }
        }

        private int RandomSeed;

        private double average;
        private bool averageCacheInvalidated;

        public double AverageScore
        {
            get
            {
                if (Failed)
                {
                    return 0;
                }

                if (averageCacheInvalidated)
                {
                    UpdateGenomes();
                    average = Population.Select(genome => genome.Score).Average();

                    averageCacheInvalidated = false;
                }

                return average;
            }
        }

        public int NumberOfGenerations { get; set; }

        public BaseGA(int populationSize, Func<GenomeType, double> scoreFunction)
        {
            this.populationSize = populationSize;
            this.scoreFunction = scoreFunction;

            this.RandomSeed = DateTime.Now.Millisecond;
            this.Random = new Random(RandomSeed);
        }

        public void Initialise()
        {
            this.Population = new List<GenomeType>(populationSize);

            foreach (var i in Enumerable.Range(0, populationSize))
            {
                var newGenome = CreateGenome();

                newGenome.Initialise();
                AddGenome(newGenome);
            }

            this.NumberOfGenerations = 0;
        }

        private void AddGenome(GenomeType genome)
        {
            population.Add(genome);
            UpdateGenome(genome);

            if (GenomeAdded != null)
            {
                GenomeAdded(this, new GenomeEventArgs<GenomeType>(genome));
            }
        }

        public void GenerationalIterate()
        {
            if (Failed)
            {
                return;
            }

            if (IterationBegin != null)
            {
                IterationBegin(this, new IterationEventArgs(NumberOfGenerations));
            }

            var generationalSelect = PerformGenerationalSelection();

            population.Clear();

            if (SelectionComplete != null)
            {
                SelectionComplete(this, new EventArgs());
            }

            foreach (var genomeToRetain in generationalSelect.ToRetain)
            {
                AddGenome(genomeToRetain);
            }

            foreach (var genomeToMutate in generationalSelect.ToMutate)
            {
                genomeToMutate.Mutate();
                AddGenome(genomeToMutate);
            }

            foreach (var parentPairs in generationalSelect.ParentPairs)
            {
                var child = parentPairs.Item1.Crossover(parentPairs.Item2);

                child.Mutate();
                AddGenome((GenomeType)child);
            }

            Update();

            if (IterationComplete != null)
            {
                IterationComplete(this, new IterationEventArgs(NumberOfGenerations));
            }

            NumberOfGenerations++;
        }

        protected abstract GenomeType CreateGenome();

        protected abstract GAGenerationalSelectionResult<GenomeType, GType, PType>
PerformGenerationalSelection();

        public void ForceUpdateGenomes()
        {
            foreach (var genome in population)
            {
                genome.Update();
                UpdateGenome(genome);
            }

            Update();
        }

        public void Update()
        {
            bestCacheInvalidated = true;
            averageCacheInvalidated = true;
        }

        public void UpdateGenomes()
        {
            foreach (var genome in population.Where(elem => elem.PhenomeExpired))
            {
                UpdateGenome(genome);
            }
        }

        private void UpdateGenome(GenomeType genome)
        {
            if (genome.PhenomeExpired)
            {
                genome.UpdatePhenome();
                genome.Score = scoreFunction(genome);

                Update();
            }
        }
    }
}