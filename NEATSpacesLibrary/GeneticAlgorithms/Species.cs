using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    /// <summary>
    /// Represents a species in a speciated genetic algorithm.
    /// </summary>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public class Species<GType, PType> 
    {
        /// <summary>
        /// The genetic algorithm this species belongs to.
        /// </summary>
        private ISpeciatedGA parent;

        /// <summary>
        /// The best score of this species in the previous iteration.
        /// </summary>
        internal double PreviousScore { get; set; }

        /// <summary>
        /// True if the members of this species can breed.
        /// </summary>
        internal bool CanBreed { get; set; }

        /// <summary>
        /// The number of iterations in which the score of this species hasn't improved.
        /// </summary>
        internal int TotalIterationsWithNoInnovation {get; set;}

        /// <summary>
        /// The average fitness of the indviduals in this species.
        /// </summary>
        private double averageFitness;
        private bool averageFitnessCacheInvalidated;
        public double AverageFitness
        {
            get
            {
                if (averageFitnessCacheInvalidated)
                {
                    parent.UpdateGenomes();

                    averageFitness = members.Select(member => member.AdjustedScore).Average();
                    averageFitnessCacheInvalidated = false;
                }

                return averageFitness;
            }
        }

        /// <summary>
        /// The best genome in this species.
        /// </summary>
        public SpeciatedGenome<GType, PType> Best
        {
            get
            {
                return Members.First();
            }
        }

        /// <summary>
        /// The representative of this species.
        /// </summary>
        private SpeciatedGenome<GType, PType> representative;

        /// <summary>
        /// The members of this species.
        /// </summary>
        /// <remarks>
        /// This list will always be ordered in descending order of the score.
        /// </remarks>
        private List<SpeciatedGenome<GType, PType>> members;
        private bool listCacheInvalidated;
        public IEnumerable<SpeciatedGenome<GType, PType>> Members
        {
            get
            {
                if (listCacheInvalidated)
                {
                    parent.UpdateGenomes();

                    members.Sort(new Comparison<SpeciatedGenome<GType, PType>>(
                                        (member1, member2) => member2.Score.CompareTo(member1.Score)));
                    listCacheInvalidated = false;
                }

                return members.AsReadOnly();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parent">The genetic algorithm this species belongs to.</param>
        /// <param name="representative">The representative of this species.</param>
        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.representative = representative;

            this.members = new List<SpeciatedGenome<GType, PType>>() { representative };
            this.CanBreed = true;

            this.PreviousScore = Best.Score;

            Update();
        }

        /// <summary>
        /// Is true if the given genome belongs to this species.
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return representative.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        /// <summary>
        /// Adds the given genome to the species.
        /// </summary>
        /// <param name="genome"></param>
        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            members.Add(genome);

            representative = genome;

            Update();
        }

        /// <summary>
        /// Marks the caches of this species as stale.
        /// </summary>
        public void Update()
        {
            listCacheInvalidated = true;
            averageFitnessCacheInvalidated = true;
        }

        /// <summary>
        /// Removes the given genome from the species.
        /// </summary>
        /// <param name="genome"></param>
        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            members.Remove(genome);
            Update();
        }

        /// <summary>
        /// The number of genomes in this species.
        /// </summary>
        public int Count
        {
            get
            {
                return members.Count;
            }
        }

        /// <summary>
        /// Removes all the genomes from this species.
        /// </summary>
        public void Clear()
        {
            members.Clear();
            Update();
        }
    }
}
