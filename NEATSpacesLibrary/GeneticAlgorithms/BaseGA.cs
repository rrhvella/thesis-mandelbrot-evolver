﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using System.Threading.Tasks;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IDebugabble
    {
        string DebugInformation();
    }

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

    public class GASteadyStateSelectionResult<GenomeType, GType, PType> : IDebugabble where GenomeType: Genome<GType, PType>
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

        public GASteadyStateSelectionResult(GenomeType parent, GenomeType partner, 
                        GenomeType individualToReplace1, GenomeType individualToReplace2)
        {
            this.Parent = parent;
            this.Partner = partner;

            this.IndividualsToReplace = new[] { individualToReplace1, individualToReplace2 };
        }

        public string DebugInformation()
        {
            var result = new StringBuilder();

            result.AppendLine("Parent: ");
            result.AppendLine(Parent.DebugInformation());

            result.AppendLine("Partner: ");
            result.AppendLine(Partner.DebugInformation());

            result.AppendLine("Individual to replace 1: ");
            result.AppendLine(IndividualsToReplace[0].DebugInformation());

            result.AppendLine("Individual to replace 2: ");
            result.AppendLine(IndividualsToReplace[1].DebugInformation());

            return result.ToString();
        }
    }

    public class GAGenerationalSelectionResult<GenomeType, GType, PType> : IDebugabble where GenomeType : Genome<GType, PType>
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

        public string DebugInformation()
        {
            var result = new StringBuilder();

            result.AppendLine("To retain unchanged: ");
            result.AppendLine(ToRetain.GenomeDebugInformation<GenomeType, GType, PType>());

            result.AppendLine("To mutate without crossover: ");
            result.AppendLine(ToMutate.GenomeDebugInformation<GenomeType, GType, PType>());

            result.AppendLine("Parent pairs: ");

            foreach (var parentPair in ParentPairs)
            {
                result.AppendLine(parentPair.Item1.DebugInformation());
                result.AppendLine(parentPair.Item2.DebugInformation());
                result.AppendLine();
            }

            return result.ToString();
        }
    }

    public abstract class BaseGA<GenomeType, GType, PType> : IGA
        where GenomeType : Genome<GType, PType>, new()
    {
        public const int DEFAULT_TOURNAMENT_SIZE = 7;

        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeAdded;
        public event EventHandler<GenomeEventArgs<GenomeType>> GenomeRemoved;

        public event EventHandler<EventArgs> SelectionComplete;
        public event EventHandler<IterationEventArgs> IterationBegin;
        public event EventHandler<IterationEventArgs> IterationComplete;

        private int iterationNumber;

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

        private IDebugabble previousSelection;
        private IList<Genome<GType, PType>> previousChildren;

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
                    average =  Population.Select(genome => genome.Score).Average();

                    averageCacheInvalidated = false;
                }

                return average; 
            }
        }

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

            foreach(var i in Enumerable.Range(0, populationSize)) 
            {
                var newGenome = new GenomeType();
                newGenome.Parent = this;

                newGenome.Initialise();
                AddGenome(newGenome);
            }
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
                IterationBegin(this, new IterationEventArgs(iterationNumber));
            }

            previousChildren = new List<Genome<GType, PType>>();
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

                previousChildren.Add(genomeToMutate);
            }

            foreach (var parentPairs in generationalSelect.ParentPairs)
            {
                var child = parentPairs.Item1.Crossover(parentPairs.Item2)[0];

                child.Mutate();
                AddGenome((GenomeType)child);

                previousChildren.Add(child);
            }

            Update();

            if (IterationComplete != null)
            {
                IterationComplete(this, new IterationEventArgs(++iterationNumber));
            }

            previousSelection = generationalSelect;
        }

        protected abstract GAGenerationalSelectionResult<GenomeType, GType, PType> PerformGenerationalSelection();

        public void SteadyStateIterate()
        {
            if (Failed)
            {
                return;
            }

            if (IterationBegin != null)
            {
                IterationBegin(this, new IterationEventArgs(iterationNumber));
            }

            var selection = PerformSteadyStateSelection();
            previousChildren = selection.Parent.Crossover(selection.Partner);

            foreach (var i in Enumerable.Range(0, selection.IndividualsToReplace.Length))
            {
                population.Remove(selection.IndividualsToReplace[i]);

                if (GenomeRemoved != null)
                {
                    GenomeRemoved(this, new GenomeEventArgs<GenomeType>(selection.IndividualsToReplace[i]));
                }

                previousChildren[i].Parent = this;
                previousChildren[i].Mutate();

                AddGenome((GenomeType)previousChildren[i]);
            }

            Update();

            if (IterationComplete != null)
            {
                IterationComplete(this, new IterationEventArgs(++iterationNumber));
            }

            previousSelection = selection;
        }

        public void Update()
        {
            bestCacheInvalidated = true;
            averageCacheInvalidated = true;
        }

        protected abstract GASteadyStateSelectionResult<GenomeType, GType, PType> PerformSteadyStateSelection();

        protected virtual string InnerDebugInformation()
        {
            var result = new StringBuilder();

            result.Append(Population.GenomeDebugInformation<GenomeType, GType, PType>());

            return result.ToString();
        }

        public string DebugInformation()
        {
            var result = new StringBuilder();

            result.AppendLine("Best: ");
            result.AppendLine(Best.DebugInformation());

            result.AppendLine();

            result.AppendLine("Previous selection: ");
            result.AppendLine(previousSelection.DebugInformation());

            result.AppendLine("Previous children: ");
            result.AppendLine(previousChildren.GenomeDebugInformation<Genome<GType, PType>, GType, PType>());

            result.AppendLine(InnerDebugInformation());

            return result.ToString();
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
