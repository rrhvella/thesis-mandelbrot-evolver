using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class Species<GType, PType> 
    {
        private SpeciatedGenome<GType, PType> representative;
        private ISpeciatedGA parent;

        public bool CanBreed { get; set; }

        public int TotalIterations {get; set;}
        private List<SpeciatedGenome<GType, PType>> members;

        public IEnumerable<SpeciatedGenome<GType, PType>> Members
        {
            get
            {
                return members.AsEnumerable();
            }
        }

        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.representative = representative;
            this.members = new List<SpeciatedGenome<GType, PType>>();
        }

        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return representative.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            members.Add(genome);
            representative = genome;
        }

        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            members.Remove(genome);

            if (genome == representative && members.Count > 0)
            {
                representative = members[0]; 
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
