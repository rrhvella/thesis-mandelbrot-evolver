using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class SpeciatedGenome<GType, PType> : Genome<GType, PType> 
    {
        public override double Score
        {
            get
            {
                return base.Score / Species.Count;
            }
        }
        public Species<GType, PType> Species;
        public abstract double CompatibilityDistance(SpeciatedGenome<GType, PType> genome);
    }
}
