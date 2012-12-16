using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
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
                    throw new ApplicationException("Attempted to retrieve score from an orphan genome. Please use a genetic algortihm");
                }

                if (PhenomeExpired)
                {
                    throw new ApplicationException("Attempted to retrieve score from a stale genome. Please force the genetic algorithm to update its genomes");
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
                    throw new ApplicationException("Attempted to retrieve phenome from an orphan genome. Please use a genetic algortihm");
                }

                if (PhenomeExpired)
                {
                    throw new ApplicationException("Attempted to retrieve phenome from a stale genome. Please force the genetic algorithm to update its genomes");
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


        /// <summary>
        /// Returns the phenome of the individual <seealso cref="MandelbrotCPPNNEATPhenome"/>
        /// </summary>
        /// <returns></returns>
        protected abstract PType GetPhenome();
        
        public abstract void Initialise();

        /// <summary>
        /// Returns the child of this genome when it mates with another individual.
        /// </summary>
        /// <param name="partner"></param>
        /// <returns></returns>
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

        
        
        /// <summary>
        /// Returns the child of this genome when it mates with another individual.
        /// </summary>
        /// <param name="partner"></param>
        /// <returns></returns>
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
