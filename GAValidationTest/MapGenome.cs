using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace GAValidationTest
{
    public class MapGenome: CollectionGenome<MapGenome, bool[], bool>
    {
        private const double FILL = 0.05;

        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        public MapGenome(): base(MUTATION_RATE, CROSSOVER_RATE)
        {
            this.GeneCollection = new bool[MapConstants.MAP_SIZE * MapConstants.MAP_SIZE];
        }

        public override void Initialise()
        {
            foreach (var i in Enumerable.Range(0, MapConstants.MAP_SIZE * MapConstants.MAP_SIZE))
            {
                if (Parent.Random.NextDouble() <= FILL)
                {
                    GeneCollection[i] = true;
                }
            }
        }

        protected override bool GetMutatedElement(bool currentValue)
        {
            return !currentValue;
        }
    }
}
