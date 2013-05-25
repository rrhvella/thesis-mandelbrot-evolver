﻿namespace GeneticAlgorithms
{
    public abstract class SpeciatedGenome<GType, PType> : Genome<GType, PType>
    {
        public double AdjustedScore
        {
            get
            {
                return Score / Species.Count;
            }
        }

        public Species<GType, PType> Species;

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