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
        public List<SpeciatedGenome<GType, PType>> Members { get; private set; }

        public Species(ISpeciatedGA parent, SpeciatedGenome<GType, PType> representative)
        {
            this.parent = parent;
            this.representative = representative;
            this.Members = new List<SpeciatedGenome<GType, PType>>();
        }

        public bool BelongsTo(SpeciatedGenome<GType, PType> genome)
        {
            return representative.CompatibilityDistance(genome) <= parent.CompatibilityDistanceThreshold;
        }

        public void Add(SpeciatedGenome<GType, PType> genome)
        {
            Members.Add(genome);
            representative = genome;
        }

        public void Remove(SpeciatedGenome<GType, PType> genome)
        {
            Members.Remove(genome);

            if (genome == representative && Members.Count > 0)
            {
                representative = Members[0]; 
            }
        }

        public int Count
        {
            get
            {
                return Members.Count;
            }
        }
    }
}
