using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.NEATSpaces;

namespace GAValidationTest
{
    public class MapGenome: Genome<Map, Map>
    {
        private const double FILL = 0.05;

        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        public MapGenome()
        {
            this.GeneCollection = MapConstants.CreateMap();
        }

        public override void Initialise()
        {
            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    if (Parent.Random.NextDouble() <= FILL)
                    {
                        GeneCollection[x, y] = true;
                    }
                }
            }
        }

        protected override Map GetPhenome()
        {
            return GeneCollection;
        }

        protected override Genome<Map, Map>[] InnerCrossover(Genome<Map, Map> partner)
        {
            var children = new MapGenome[] { new MapGenome(), new MapGenome() };

            foreach(var i in Enumerable.Range(0, GeneCollection.Length))
            {
                if (Parent.Random.NextDouble() <= CROSSOVER_RATE)
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
            foreach(var i in Enumerable.Range(0, GeneCollection.Length))
            {
                if(Parent.Random.NextDouble() <= MUTATION_RATE) 
                {
                    GeneCollection[i] = !GeneCollection[i];
                }
            }
        }

        public override Genome<Map, Map> InnerCopy()
        {
            var result = (MapGenome)this.MemberwiseClone();
            result.GeneCollection = result.GeneCollection.Copy(); ;

            return result;
        }
    }
}
