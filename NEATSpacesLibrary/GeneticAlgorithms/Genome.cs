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
            internal set;
        }

        public virtual double AdjustedScore
        {
            get
            {
                return Score;
            }
        }

        public bool PhenomeExpired
        {
            get;
            set;
        }

        public IGA Parent
        {
            get;
            internal set;
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
        protected abstract Genome<GType, PType>[] InnerCrossover(Genome<GType, PType> partner);
        protected abstract void InnerMutate();

        public void UpdatePhenome()
        {
            Phenome = GetPhenome();
            PhenomeExpired = false;
        }

        public void Update()
        {
            PhenomeExpired = true;
            Parent.Update();
        }

        public Genome<GType, PType>[] Crossover(Genome<GType, PType> partner)
        {
            var children = InnerCrossover(partner);
            Update();

            return children;
        }

        public void Mutate()
        {
            InnerMutate();
            Update();
        }
    }
}
