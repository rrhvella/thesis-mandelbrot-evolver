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

    public class GASelectionResult<GenomeType, GType, PType> where GenomeType: Genome<GType, PType>
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

        public Random Random
        {
            get;
            private set;
        }

        private GenomeType best;
        private bool bestCacheExpired;
        private Func<GenomeType, double> scoreFunction;

        private GASelectionResult<GenomeType, GType, PType> previousSelection;
        private Genome<GType, PType>[] previousChildren;

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

            this.Random = new Random();
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
            previousSelection = PerformSelection();
            previousChildren = previousSelection.Parent.Crossover(previousSelection.Partner);

            foreach (var i in Enumerable.Range(0, previousSelection.IndividualsToReplace.Length))
            {
                Population.Remove(previousSelection.IndividualsToReplace[i]);

                if (GenomeRemoved != null)
                {
                    GenomeRemoved(this, new GenomeEventArgs<GenomeType>(previousSelection.IndividualsToReplace[i]));
                }

                Population.Add((GenomeType)previousChildren[i]);

                previousChildren[i].Parent = this;
                previousChildren[i].Mutate();

                if (GenomeAdded != null)
                {
                    GenomeAdded(this, new GenomeEventArgs<GenomeType>((GenomeType)previousChildren[i]));
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

        protected abstract GASelectionResult<GenomeType, GType, PType> PerformSelection();

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
    }
}
