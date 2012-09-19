using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing.Imaging;

namespace GAValidationTest
{
    public class Program
    {
        private const int NUMBER_OF_GENERATIONS = 250;
        private const int MATING_EVENTS_PER_GENERATION = 2000;
        private const int POPULATION_SIZE = 120;
        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        public class MapGenome: Genome<Map, Map>
        {
            private const double FILL = 0.05;
            private const int MANDATORY_CHECKPOINT_LEVEL = 3;
            private static readonly MapNode[] CHECKPOINTS = new[] { new MapNode(6, 24), new MapNode(12, 18), 
                                                            new MapNode(18, 12), new MapNode(24, 6) };
            private static readonly MapNode START_NODE = new MapNode(0, 0);
            private static readonly MapNode END_NODE = new MapNode(29, 29);

            private double FILL_PARAMETER = 0.05;

            private const int MAP_SIZE = 30;
            private Random random;

            public MapGenome()
            {
                this.GeneCollection = new Map(MAP_SIZE, MAP_SIZE, START_NODE, END_NODE, CHECKPOINTS, MANDATORY_CHECKPOINT_LEVEL);
                this.random = new Random();
            }

            public override void Initialise()
            {
                var random = new Random();

                foreach (var x in Enumerable.Range(0, MAP_SIZE))
                {
                    foreach (var y in Enumerable.Range(0, MAP_SIZE))
                    {
                        if (random.NextDouble() <= FILL_PARAMETER)
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
                    if (random.NextDouble() <= CROSSOVER_RATE)
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
                    if(random.NextDouble() <= MUTATION_RATE) 
                    {
                        GeneCollection[i] = !GeneCollection[i];
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine(String.Format("Initialising the population..."));

            var testGA = new SteadyStateGA<MapGenome, Map, Map>(POPULATION_SIZE, genome => genome.Phenome.DistanceFromStartToEnd);
            testGA.Initialise();

            var data = new List<double>();

            foreach (var i in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
            {
                Console.Clear();
                Console.WriteLine(String.Format("{0} out of {1} generations completed.", i, NUMBER_OF_GENERATIONS));

                var averageFitness = 0.0;

                foreach (var j in Enumerable.Range(0, MATING_EVENTS_PER_GENERATION))
                {
                    testGA.Iterate();
                    averageFitness += testGA.Best.Score;
                }

                averageFitness /= MATING_EVENTS_PER_GENERATION;

                data.Add(averageFitness);
            }
            
            testGA.Best.Phenome.Image.Save("output.png", ImageFormat.Png);

            FileStream file = new FileStream("output.csv", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);

            foreach (var i in Enumerable.Range(0, data.Count))
            {
                writer.WriteLine(String.Format("{0},{1}", i, data[i]));
            }

            writer.Close();
            file.Close();
        }
    }
}
