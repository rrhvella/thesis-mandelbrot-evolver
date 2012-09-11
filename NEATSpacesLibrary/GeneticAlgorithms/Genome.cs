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
            get;
            set;
        }

        public virtual double AdjustedScore
        {
            get
            {
                return Score;
            }
        }

        public IGA Parent
        {
            get;
            set;
        }

        public PType Phenome
        {
            get;
            private set;
        }

        public GType GeneCollection
        {
            get;
            protected set;
        }

        protected abstract PType GetPhenome();
        
        public abstract void Initialise();
        public abstract Genome<GType, PType>[] Crossover(Genome<GType, PType> partner);
        
        public abstract void Mutate();

        public void Update()
        {
            Phenome = GetPhenome();
        }
    }
}
