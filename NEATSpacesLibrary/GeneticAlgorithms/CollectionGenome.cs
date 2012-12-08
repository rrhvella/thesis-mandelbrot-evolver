using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class CollectionGenome<GenomeType, GType, EType> : Genome<GType, GType>
        where GType : IList<EType>, ICloneable
        where GenomeType : CollectionGenome<GenomeType, GType, EType>, new()
    {
        private double mutationRate;
        private double crossoverRate;

        public CollectionGenome(double mutationRate, double crossoverRate)
        {
            this.mutationRate = mutationRate;
            this.crossoverRate = crossoverRate;
        }
        protected override GType GetPhenome()
        {
            return GeneCollection;
        }

        protected override Genome<GType, GType> InnerCrossover(Genome<GType, GType> partner)
        {
            var child = new GenomeType();

            foreach(var i in Enumerable.Range(0, GeneCollection.Count))
            {
                if (Parent.Random.NextDouble() <= crossoverRate)
                {
                    child.GeneCollection[i] = GeneCollection[i];
                } 
                else 
                {
                    child.GeneCollection[i] = partner.GeneCollection[i];
                }
            }

            return child;
        }

        protected override void InnerMutate()
        {
            foreach(var i in Enumerable.Range(0, GeneCollection.Count))
            {
                if(Parent.Random.NextDouble() <= mutationRate) 
                {
                    GeneCollection[i] = GetMutatedElement(GeneCollection[i]);
                }
            }
        }

        protected abstract EType GetMutatedElement(EType currentValue);

        public override Genome<GType, GType> InnerCopy()
        {
            var result = (GenomeType)this.MemberwiseClone();
            result.GeneCollection = (GType)result.GeneCollection.Clone();

            return result;
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }
    }
}
