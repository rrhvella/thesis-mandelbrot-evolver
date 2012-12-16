using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using System.Threading.Tasks;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    /// <summary>
    /// Represents an event fired by a genetic algorithm.
    /// </summary>
    /// <typeparam name="GenomeType">The type of the genomes in the genetic algorithm.</typeparam>
    public class GenomeEventArgs<GenomeType> : EventArgs
    {
        /// <summary>
        /// The genome on which the event transpired.
        /// </summary>
        public GenomeType Genome 
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="genome">The genome on which the event transpired.</param>
        public GenomeEventArgs(GenomeType genome) 
        {
            this.Genome = genome;
        }
    }

    /// <summary>
    /// Represents an iteration related event fired by a genetic algorithm.
    /// </summary>
    public class IterationEventArgs : EventArgs
    {
        /// <summary>
        /// The iteration number when the event transpired.
        /// </summary>
        public int IterationNumber 
        {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="iterationNumber">The iteration number when the event transpired.</param>
        public IterationEventArgs(int iterationNumber) 
        {
            this.IterationNumber = iterationNumber;
        }
    }


    /// <summary>
    /// Represents the results of a generational selection.
    /// </summary>
    /// <typeparam name="GenomeType">The genome type. </typeparam>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public class GAGenerationalSelectionResult<GenomeType, GType, PType> 
            where GenomeType : Genome<GType, PType>
    {
        /// <summary>
        /// The individuals which will be retained without modification.
        /// </summary>
        public IList<GenomeType> ToRetain 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The individuals which will be mutated.
        /// </summary>
        public IList<GenomeType> ToMutate 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The pairs of individuals which will be mated.
        /// </summary>
        public IList<Tuple<GenomeType, GenomeType>> ParentPairs 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The total number of individuals in the next population if all the requests in this selection 
        /// are processed.
        /// </summary>
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

    /// <summary>
    /// Base class for all genetic algorithms.
    /// </summary>
    /// <typeparam name="GenomeType">The genome type. </typeparam>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public abstract class BaseGA<GenomeType, GType, PType> : IGA
        where GenomeType : Genome<GType, PType>, new()
    {
        /// <summary>
        /// Fired when a new genome is added. 
        /// </summary>
        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeAdded;

        /// <summary>
        /// Fired when a selection has been performed. 
        /// </summary>
        public event EventHandler<EventArgs> SelectionComplete;

        /// <summary>
        /// Fired before an iteration begins. 
        /// </summary>
        public event EventHandler<IterationEventArgs> IterationBegin;

        /// <summary>
        /// Fired after an iteration has been completed. 
        /// </summary>
        public event EventHandler<IterationEventArgs> IterationComplete;

        /// <summary>
        /// The total number of individuals in the population.
        /// </summary>
        private int populationSize;

        /// <summary>
        /// The current population.
        /// </summary>
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

        /// <summary>
        /// Is true if the GA can not perform anymore generations.
        /// </summary>
        public virtual bool Failed
        {
            get 
            { 
                return false;  
            }
        }

        /// <summary>
        /// The random number generator for the GA.
        /// </summary>
        public Random Random
        {
            get;
            private set;
        }

        /// <summary>
        /// The function used to determine the fitness of an individual. 
        /// </summary>
        private Func<GenomeType, double> scoreFunction;

        /// <summary>
        /// The genome which currently has the highest fitness.
        /// </summary>
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

        /// <summary>
        /// The seed of the random number generator.
        /// </summary>
        private int RandomSeed;

        /// <summary>
        /// The average fitness of the population.
        /// </summary>
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
                    average =  Population.Select(genome => genome.Score).Average();

                    averageCacheInvalidated = false;
                }

                return average; 
            }
        }

        /// <summary>
        /// The total number of generations performed by this GA.
        /// </summary>
        public int NumberOfGenerations { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="populationSize">The number of individuals in the population.</param>
        /// <param name="scoreFunction">The function used to determine the fitness of an individual.
        /// </param>
        public BaseGA(int populationSize, Func<GenomeType, double> scoreFunction)
        {
            this.populationSize = populationSize;
            this.scoreFunction = scoreFunction;

            this.RandomSeed = DateTime.Now.Millisecond;
            this.Random = new Random(RandomSeed);
        }

        /// <summary>
        /// Initialises the GA's population.
        /// </summary>
        public void Initialise()
        {
            this.Population = new List<GenomeType>(populationSize);

            foreach(var i in Enumerable.Range(0, populationSize)) 
            {
                var newGenome = new GenomeType();
                newGenome.Parent = this;

                newGenome.Initialise();
                AddGenome(newGenome);
            }

            this.NumberOfGenerations = 0;
        }

        /// <summary>
        /// Adds the given genome to the population.
        /// </summary>
        /// <param name="genome"></param>
        private void AddGenome(GenomeType genome) 
        {
            population.Add(genome);
            UpdateGenome(genome);

            if (GenomeAdded != null)
            {
                GenomeAdded(this, new GenomeEventArgs<GenomeType>(genome));
            }
        }

        /// <summary>
        /// Performs a generational iteration of the GA.
        /// </summary>
        public void GenerationalIterate()
        {
            //If this GA has failed, not perform any more iterations.
            if (Failed)
            {
                return;
            }

            //Fire the iteration begin event.
            if (IterationBegin != null)
            {
                IterationBegin(this, new IterationEventArgs(NumberOfGenerations));
            }

            //Perform the selection.
            var generationalSelect = PerformGenerationalSelection();

            //Rebuild the population from the requests made by the selection.
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

            //Mark all caches as stale.
            Update();

            //Fire the iteration end event.
            if (IterationComplete != null)
            {
                IterationComplete(this, new IterationEventArgs(NumberOfGenerations));
            }

            //Increment the number of generations.
            NumberOfGenerations++;
        }

        /// <summary>
        /// Returns the results of a generational selection.
        /// </summary>
        /// <returns></returns>
        protected abstract GAGenerationalSelectionResult<GenomeType, GType, PType> 
                PerformGenerationalSelection();

        /// <summary>
        /// Forcibly updates all the genomes, even if their scores and phenomes are not 
        /// stale.
        /// </summary>
        public void ForceUpdateGenomes()
        {
            foreach (var genome in population)
            {
                genome.Update();
                UpdateGenome(genome);
            }

            Update();
        }

        /// <summary>
        /// Marks the GA's caches as stale.
        /// </summary>
        public void Update()
        {
            bestCacheInvalidated = true;
            averageCacheInvalidated = true;
        }

        /// <summary>
        /// Goes through all the genomes and updates their phenomes and scores if they are stale.
        /// </summary>
        public void UpdateGenomes()
        {
            foreach (var genome in population.Where(elem => elem.PhenomeExpired))
            {
                UpdateGenome(genome);
            }
        }

        /// <summary>
        /// Updates the phenome and score of the given genome, if it is stale.
        /// </summary>
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
