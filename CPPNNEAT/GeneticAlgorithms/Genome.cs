using System;

namespace CPPNNEAT.GeneticAlgorithms
{
    public abstract class Genome<GType, PType>
    {
        private double score;

        public double Score
        {
            get
            {
                if (Parent == null)
                {
                    throw new ApplicationException("Attempted to retrieve score from an orphan genome. " +
                                                "Please use a genetic algortihm");
                }

                if (PhenomeExpired)
                {
                    throw new ApplicationException("Attempted to retrieve score from a stale genome. " +
                                                "Please force the genetic algorithm to update its genomes");
                }

                return score;
            }
            internal set
            {
                score = value;
            }
        }

        public bool PhenomeExpired
        {
            get;
            set;
        }

        public IGA Parent
        {
            get;
            internal set;
        }

        private PType phenome;

        public PType Phenome
        {
            get
            {
                if (Parent == null)
                {
                    throw new ApplicationException("Attempted to retrieve score from an orphan genome. " +
                                                "Please use a genetic algortihm");
                }

                if (PhenomeExpired)
                {
                    throw new ApplicationException("Attempted to retrieve score from a stale genome. " +
                                                "Please force the genetic algorithm to update its genomes");
                }

                return phenome;
            }
            private set
            {
                phenome = value;
            }
        }

        public GType GeneCollection
        {
            get;
            protected set;
        }

        protected abstract PType GetPhenome();

        public abstract void Initialise();

        protected abstract Genome<GType, PType> InnerCrossover(Genome<GType, PType> partner);

        protected abstract void InnerMutate();

        public void UpdatePhenome()
        {
            Phenome = GetPhenome();
            PhenomeExpired = false;
        }

        public virtual void Update()
        {
            PhenomeExpired = true;
            Parent.Update();
        }

        public Genome<GType, PType> Crossover(Genome<GType, PType> partner)
        {
            return InnerCrossover(partner);
        }

        public void Mutate()
        {
            InnerMutate();
            Update();
        }

        public abstract Genome<GType, PType> InnerCopy();

        public Genome<GType, PType> Copy()
        {
            var result = InnerCopy();
            result.Update();

            return result;
        }
    }
}