using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class Species<GType, PType> 
    {
        private ISpeciatedGA parent;

        internal double PreviousScore { get; set; }
        internal bool CanBreed { get; set; }
        internal int TotalIterationsWithNoInnovation {get; set;}

        private List<SpeciatedGenome<GType, PType>> members;

        public SpeciatedGenome<GType, PType> Best { get; private set; }

        public IEnumerable<SpeciatedGenome<GType, PType>> Members
        {
            get
            {
                return members;
            }
        }

        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.Best = representative;
            this.PreviousScore = Best.Score;

            this.members = new List<SpeciatedGenome<GType, PType>>() { representative };
            this.CanBreed = true;
        }

        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return Best.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            members.Add(genome);

            if (genome.Score > Best.Score)
            {
                Best = genome;
            }
        }

        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            members.Remove(genome);

            if (genome == Best && members.Count > 0)
            {
                Best = members.MaxBy(member => member.Score);
            }
        }

        public int Count
        {
            get
            {
                return members.Count;
            }
        }
    }
}
