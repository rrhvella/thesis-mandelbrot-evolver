using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace GAValidationTest
{
    public class Program
    {
        private const int NUMBER_OF_GENERATIONS = 250;
        private const int MATING_EVENTS_PER_GENERATION = 2000;
        private const int POPULATION_SIZE = 120;
        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        public class MapGenome: Genome<bool, Map> 
        {
            private const double FILL = 0.05;
            private const int MANDATORY_CHECKPOINT_LEVEL = 3;
            private static readonly MapNode[] CHECKPOINTS = new[] { new MapNode(6, 24), new MapNode(12, 18), 
                                                            new MapNode(18, 12), new MapNode(24, 6) };
            private static readonly MapNode START_NODE = new MapNode(0, 0);
            private static readonly MapNode END_NODE = new MapNode(29, 29);

            private double FILL_PARAMETER = 0.05;

            private const int MAP_SIZE = 30;

            private Map genome;

            public void Initialise()
            {
                genome = new Map(MAP_SIZE, MAP_SIZE);
                var random = new Random();

                foreach (var x in Enumerable.Range(0, MAP_SIZE))
                {
                    foreach (var y in Enumerable.Range(0, MAP_SIZE))
                    {
                        if (random.NextDouble() < FILL_PARAMETER)
                        {
                            genome[x, y] = true;
                        }
                    }
                }
            }

            protected override double GetScore()
            {
                return Phenotype.DistanceFromStartToEnd(START_NODE, END_NODE, 
                                CHECKPOINTS, MANDATORY_CHECKPOINT_LEVEL);
            }
        }

        public static void Main(string[] args)
        {
            var testGA = new GA<MapGenome>(POPULATION_SIZE);

            testGA.MutationRate = MUTATION_RATE;
            testGA.CrossoverRate = CROSSOVER_RATE;

            var data = new List<double>();

            foreach (var i in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
            {
                var averageFitness = 0.0;

                foreach (var j in Enumerable.Range(0, MATING_EVENTS_PER_GENERATION))
                {
                    testGA.Iterate();
                    averageFitness += testGA.Best.Score;
                }

                averageFitness /= MATING_EVENTS_PER_GENERATION;

                data.Add(averageFitness);
            }
        }
    }
}
