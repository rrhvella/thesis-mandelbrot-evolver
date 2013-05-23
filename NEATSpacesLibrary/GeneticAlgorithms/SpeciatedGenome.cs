using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class SpeciatedGenome<GType, PType> : Genome<GType, PType> 
    {
        public override double AdjustedScore
        {
            get
            {
                return Score / Species.Count;
            }
        }
        public Species<GType, PType> Species;
        private CPPNNEAT.CPPNNEATGA parent;

        public SpeciatedGenome(IGA parent): base(parent)
        {
        }

        public abstract double CompatibilityDistance(SpeciatedGenome<GType, PType> genome);

        public override void Update()
        {
            if (Species != null)
            {
                Species.Update();
            }

            base.Update();
        }
    }
}
