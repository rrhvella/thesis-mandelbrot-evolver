using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class Genome<GType, PType> 
    {
        public virtual double Score
        {
            get;
            private set;
        }

        public PType Phenome
        {
            get;
            private set;
        }

        public GType GeneticCode
        {
            get;
            protected set;
        }

        protected abstract double GetScore();
        protected abstract PType GetPhenome();
        
        public abstract void Initialise();
        public abstract Genome<GType, PType>[] Crossover(Genome<GType, PType> partner);
        
        public abstract void Mutate(double mutationProbability);

        public void Update()
        {
            Phenome = GetPhenome();
            Score = GetScore();
        }
    }
}
