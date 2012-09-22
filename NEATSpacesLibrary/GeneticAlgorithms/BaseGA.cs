using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using System.Threading.Tasks;

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
        public event EventHandler<EventArgs> IterationBegin;
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

        private IDebugabble previousSelection;
        private IList<Genome<GType, PType>> previousChildren;

        public GenomeType Best
        {
            get
            {
                if (bestCacheExpired)
                {
                    UpdateGenomes();
                    best = Population.MaxBy(genome => genome.Score);
                }

                return best;
            }
            private set
            {
                best = value;
            }
        }

        public BaseGA(int populationSize, Func<GenomeType, double> scoreFunction)
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
                newGenome.Update();

                if (GenomeAdded != null)
                {
                    GenomeAdded(this, new GenomeEventArgs<GenomeType>(newGenome));
                }
            }
        }

        private void AddGenome(GenomeType genome) 
        {
            Population.Add(genome);

            if (GenomeAdded != null)
            {
                GenomeAdded(this, new GenomeEventArgs<GenomeType>(genome));
            }
        }

        public void GenerationalIterate()
        {

            if (IterationBegin != null)
            {
                IterationBegin(this, new EventArgs());
            }

            previousChildren = new List<Genome<GType, PType>>();
            var generationalSelect = PerformGenerationalSelection();

            Population.Clear();

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
                IterationComplete(this, new EventArgs());
            }

            previousSelection = generationalSelect;
        }

        protected abstract GAGenerationalSelectionResult<GenomeType, GType, PType> PerformGenerationalSelection();

        public void SteadyStateIterate()
        {
            var selection = PerformSteadyStateSelection();
            previousChildren = selection.Parent.Crossover(selection.Partner);

            foreach (var i in Enumerable.Range(0, selection.IndividualsToReplace.Length))
            {
                Population.Remove(selection.IndividualsToReplace[i]);

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
                IterationComplete(this, new EventArgs());
            }

            previousSelection = selection;
        }

        public void Update()
        {
            bestCacheExpired = true;
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
            foreach (var genome in Population.Where(elem => elem.PhenomeExpired))
            {
                if (genome.PhenomeExpired)
                {
                    genome.UpdatePhenome();
                    genome.Score = scoreFunction(genome);
                }
            }
        }
    }
}
