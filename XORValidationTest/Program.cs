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
            var testGA = new CPPNNEATGA<XORTestGenome>(150);
            
            //Perform 100 tests, recording the average number of generations, hidden nodes for 
            //the final network, and enabled genes for the final genome.
            var averageGenerations = 0f;
            var averageHiddenNodes = 0f;
            var averageEnabledGenes = 0f;
            
            foreach(var i in Enumerable.Range(0, NUMBER_OF_RUNS))
            {
                var generations = 0;
                while (testGA.Best.Score < OPTIMAL_SCORE)
                {
                   testGA.Iterate();
                   generations++;
                }
                
                var best = testGA.Best;

                averageGenerations += generations;
                averageHiddenNodes += (from node in best.Phenotype.Nodes where node.Type == NeuronType.Hidden select node).Count();
                averageEnabledGenes += (from gene in best.Genes where gene.Enabled select gene).Count();
            }

            averageGenerations /= NUMBER_OF_RUNS; 
            averageHiddenNodes /= NUMBER_OF_RUNS; 
            averageEnabledGenes /= NUMBER_OF_RUNS; 

            //Print the results to the user.
            Console.WriteLine("Average number of generations: " + averageGenerations);
            Console.WriteLine("Average number of hidden nodes: " + averageHiddenNodes);
            Console.WriteLine("Average number of enabled genes: " + averageEnabledGenes);
        }
    }
}
