using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class Genome<GType, PType>
    {
        public double Score
        {
            get
            {
                return 0;
            }
        }

        public PType Phenotype
        {
            get
            {
                return default(PType);
            }
        }

        public GType this[int i] 
        {
            get 
            {
                return default(GType);
            }
        }

        protected abstract double GetScore();
    }
}
