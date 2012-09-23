using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public static class GenomeListExtensions
    {
        public static string GenomeDebugInformation<GenomeType, GType, PType>(this IEnumerable<GenomeType> self)
            where GenomeType : Genome<GType, PType>
        {
            var result = new StringBuilder();

            result.AppendLine("Genomes: ");

            result.AppendLine();

            foreach (var genome in self)
            {
                result.AppendLine(genome.DebugInformation());
            }

            return result.ToString();
        }
    }

    public abstract class Genome<GType, PType> : IDebugabble 
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

        public virtual void Update()
        {
            PhenomeExpired = true;
            Parent.Update();
        }

        public Genome<GType, PType>[] Crossover(Genome<GType, PType> partner)
        {
            var children = InnerCrossover(partner);

            foreach (var child in children)
            {
                child.Update();
            }

            return children;
        }

        public void Mutate()
        {
            InnerMutate();
            Update();
        }

        protected virtual string InnerDebugInformation()
        {
            return this.ToString();
        }

        public string DebugInformation()
        {
            var result = new StringBuilder();

            result.Append("Score: ");
            result.AppendLine(Score.ToString());

            result.Append("Adjusted Score: ");
            result.AppendLine(AdjustedScore.ToString());

            result.AppendLine("Genome: ");
            result.AppendLine(InnerDebugInformation());

            return result.ToString();
        }

        public abstract Genome<GType, PType> InnerCopy();

        public Genome<GType, PType> Copy()
        {
            var result = InnerCopy();
            result.Update();

            return result;
        }
    }
}
