using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CPPNNEAT.GeneticAlgorithms
{
    /// <summary>
    /// Base class for genomes in a genetic algorithm.
    /// </summary>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public abstract class Genome<GType, PType>  
    {
        /// <summary>
        /// The genome's fitness.
        /// </summary>
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

        /// <summary>
        /// Is true if the genome's phenome is stale.
        /// </summary>
        public bool PhenomeExpired
        {
            get;
            set;
        }

        /// <summary>
        /// The genetic algorithm this genome belongs to.
        /// </summary>
        public IGA Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// The phenome derived from this genome.
        /// </summary>
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

        /// <summary>
        /// The gene sequence of the genome.
        /// </summary>
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
        
        /// <summary>
        /// Initialises this genome to the default values.
        /// </summary>
        public abstract void Initialise();

        /// <summary>
        /// Returns the child of this genome when it mates with another individual.
        /// </summary>
        /// <param name="partner"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method contains the actual implementation, which is handled by the derivatives
        /// of this class.
        /// </remarks>
        protected abstract Genome<GType, PType> InnerCrossover(Genome<GType, PType> partner);

        /// <summary>
        /// Mutates the genes in the genome.
        /// </summary>
        /// <remarks>
        /// This method contains the actual implementation, which is handled by the derivatives
        /// of this class.
        /// </remarks>
        protected abstract void InnerMutate();

        /// <summary>
        /// Updates the genome's phenome.
        /// </summary>
        public void UpdatePhenome()
        {
            Phenome = GetPhenome();
            PhenomeExpired = false;
        }

        /// <summary>
        /// Updates the phenome of the genome and informs its containers that it has been
        /// updated.
        /// </summary>
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

        /// <summary>
        /// Mutates the genes in the genome.
        /// </summary>
        public void Mutate()
        {
            InnerMutate();
            Update();
        }

        /// <summary>
        /// Returns a copy of this genome.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method contains the actual implementation, which is handled by the derivatives
        /// of this class.
        /// </remarks>
        public abstract Genome<GType, PType> InnerCopy();

        /// <summary>
        /// Returns a copy of this genome.
        /// </summary>
        /// <returns></returns>
        public Genome<GType, PType> Copy()
        {
            var result = InnerCopy();
            result.Update();

            return result;
        }
    }
}
