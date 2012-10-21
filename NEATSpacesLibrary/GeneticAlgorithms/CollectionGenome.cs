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

        protected override Genome<GType, GType>[] InnerCrossover(Genome<GType, GType> partner)
        {
            var children = new GenomeType[] { new GenomeType(), new GenomeType() };

            foreach(var i in Enumerable.Range(0, GeneCollection.Count))
            {
                if (Parent.Random.NextDouble() <= crossoverRate)
                {
                    children[0].GeneCollection[i] = GeneCollection[i];
                    children[1].GeneCollection[i] = partner.GeneCollection[i];
                } 
                else 
                {
                    children[1].GeneCollection[i] = GeneCollection[i];
                    children[0].GeneCollection[i] = partner.GeneCollection[i];
                }
            }

            return children;
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
