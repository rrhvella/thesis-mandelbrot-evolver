using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace XORValidationTest
{
    public class Program
    {
        private const int NUMBER_OF_RUNS = 100;
        private static double OPTIMAL_SCORE = 16;
        private static int POPULATION_SIZE = 250;

        private static double MUTATION_RATE = 0.8;
        private static double INTERSPECIES_MATING_RATE = 0.001;
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;
        private static int NO_INNOVATION_THRESHOLD = 30000;

        private static int MATING_EVENTS_PER_GENERATION = 2000;
        
        private class XORTestGenome : CPPNNEATGenome
        {
            private static readonly double[] CORRECT_RESULT = new double[] {0, 1, 1, 0};
            
            private double[] GetResult()
            {
                return (from num in Enumerable.Range(0, 4) 
                        select GetActivation(new double[] {num / 2, num % 2})).ToArray();
            }
            
            protected override double GetScore()
            {
                return Math.Pow(4 - GetResult().Zip(CORRECT_RESULT, (a, t) => Math.Abs(t - a)).Sum(), 2);
            }
        }
        
        public static void Main(string[] args)
        {
            //Initialise the GA.
            var testGA = new SpeciatedSteadyStateGA<XORTestGenome>(POPULATION_SIZE);

            testGA.MutationRate = MUTATION_RATE;
            testGA.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;
            testGA.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            testGA.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;
            
            //Perform 100 tests, recording the average number of generations, hidden nodes for 
            //the final network, and enabled genes for the final genome.
            var averageMatingEvents = 0.0;
            var averageHiddenNodes = 0.0;
            var averageEnabledGenes = 0.0;
            
            foreach(var i in Enumerable.Range(0, NUMBER_OF_RUNS))
            {
                var matingEvents = 0;
                while (testGA.Best.Score < OPTIMAL_SCORE)
                {
                   testGA.Iterate();
                   matingEvents++;
                }
                
                var best = testGA.Best;

                averageMatingEvents += matingEvents;
                averageHiddenNodes += (from node in best.Phenome.Nodes where node.Type == NeuronType.Hidden select node).Count();
                averageEnabledGenes += (from gene in best.Genes where gene.Enabled select gene).Count();
            }

            averageMatingEvents /= NUMBER_OF_RUNS; 
            averageHiddenNodes /= NUMBER_OF_RUNS; 
            averageEnabledGenes /= NUMBER_OF_RUNS; 

            //Print the results to the user.
            Console.WriteLine("Average number of generations: " + averageMatingEvents / MATING_EVENTS_PER_GENERATION);
            Console.WriteLine("Average number of hidden nodes: " + averageHiddenNodes);
            Console.WriteLine("Average number of enabled genes: " + averageEnabledGenes);
        }
    }
}
