using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithms
{
    public class Species<GType, PType>
    {
        private ISpeciatedGA parent;

        internal double PreviousScore { get; set; }

        internal bool CanBreed { get; set; }

        internal int TotalIterationsWithNoInnovation { get; set; }

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

        public SpeciatedGenome<GType, PType> Best
        {
            get
            {
                return Members.First();
            }
        }

        private SpeciatedGenome<GType, PType> representative;

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

        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.representative = representative;

            this.members = new List<SpeciatedGenome<GType, PType>>() { representative };
            this.CanBreed = true;

            this.PreviousScore = Best.Score;

            Update();
        }

        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return representative.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            members.Add(genome);

            representative = genome;

            Update();
        }

        public void Update()
        {
            listCacheInvalidated = true;
            averageFitnessCacheInvalidated = true;
        }

        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            members.Remove(genome);
            Update();
        }

        public int Count
        {
            get
            {
                return members.Count;
            }
        }

        public void Clear()
        {
            members.Clear();
            Update();
        }
    }
}