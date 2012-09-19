﻿using System;
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
        private static int POPULATION_SIZE = 150;

        private static double INTERSPECIES_MATING_RATE = 0.001;
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;
        private static int NO_INNOVATION_THRESHOLD = 30000;

        private static int MATING_EVENTS_PER_GENERATION = 2000;
        private static readonly double[] CORRECT_RESULT = new double[] {0, 1, 1, 0};

        private static double WEIGHT_MUTATION_RATE = 0.8;
        private static double NEW_NEURON_RATE = 0.03;
        private static double NEW_LINK_RATE = 0.05;
        private static double NO_CHANGE_RATE = 0.12;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 2;
        private static double MAX_WEIGHT = 5;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;
        private const double MATCHING_GENES_WEIGHT = 0.4;

        private const int OUTPUT_ACTIVATION_PRECISION = 8;
        private const int DEBUG_PRECISION = 8;

        
        public static void Main(string[] args)
        {
            var numberOfFailures = 0;

            var totalMatingEvents = 0.0;
            var totalHiddenNodes = 0.0;
            var totalEnabledGenes = 0.0;

            var neuronCounts = new List<int>();
            
            var averageMatingEvents = 0.0; 
            var averageHiddenNodes = 0.0; 
            var averageEnabledGenes = 0.0; 

            var averageNeuronCount = 0.0;
            var STDDEVNeuronCount = 0.0;

            Console.WriteLine(String.Format("0 out of {0} runs completed.", NUMBER_OF_RUNS));

            foreach(var i in Enumerable.Range(0, NUMBER_OF_RUNS)) 
            {
                //Initialise the GA.
                var testGA = new CPPNNEATGA(2, POPULATION_SIZE, FitnessFunction, 
                    new List<Func<double,double>>() {
                        CPPNActivationFunctions.SteepenedSigmoidActivationFunction
                    });

                testGA.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;
                testGA.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
                testGA.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;

                testGA.WeightMutationRate = WEIGHT_MUTATION_RATE;
                testGA.NewNeuronRate = NEW_NEURON_RATE;
                testGA.NewLinkRate = NEW_LINK_RATE;
                testGA.NoChangeRate = NO_CHANGE_RATE;
                
                testGA.DisableGeneRate = DISABLE_GENE_RATE;

                testGA.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
                testGA.MaxPerturbation = MAX_PERTURBATION;
                testGA.MaxWeight = MAX_WEIGHT;

                testGA.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
                testGA.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
                testGA.MatchingGenesWeight = MATCHING_GENES_WEIGHT;

                testGA.Initialise();

                var matingEvents = 0;

                while (testGA.Best.Score < OPTIMAL_SCORE && !testGA.Failed)
                {
                    testGA.Iterate();
                    matingEvents++;

                    if(matingEvents % MATING_EVENTS_PER_GENERATION == 0) 
                    {
                        Console.Clear();
                        Console.WriteLine(String.Format("{0} out of {1} runs completed.", i, NUMBER_OF_RUNS));
                        Console.WriteLine(matingEvents / MATING_EVENTS_PER_GENERATION + " generations completed for current run.");
                        Console.WriteLine();
                        Console.WriteLine(String.Format("Current fitness: {0:f2}", testGA.Best.Score));
                        Console.WriteLine();
                        Console.WriteLine("Average number of generations: " + averageMatingEvents / MATING_EVENTS_PER_GENERATION);
                        Console.WriteLine("Average number of hidden nodes: " + averageHiddenNodes);
                        Console.WriteLine("Average number of enabled genes: " + averageEnabledGenes);
                        Console.WriteLine("Standard deviation of neuron count: " + STDDEVNeuronCount);
                        Console.WriteLine();
                        Console.WriteLine("Number of failures: " + numberOfFailures);
                    }
                }
                
                var best = testGA.Best;
                var runsCompleted = i + 1;

                if (testGA.Failed)
                {
                    numberOfFailures++;
                }
                else
                {
                    totalMatingEvents += matingEvents;

                    totalHiddenNodes += (from hiddenNeuron in best.Phenome.Neurons 
                                         where hiddenNeuron is CPPNHiddenNeuron 
                                         select hiddenNeuron).Count();

                    totalEnabledGenes += (from gene in best.GeneCollection.LinkGenes 
                                          where gene.Enabled 
                                          select gene).Count();

                    neuronCounts.Add(best.GeneCollection.Phenome.Neurons.Count());
                }

                averageMatingEvents = totalMatingEvents / runsCompleted; 
                averageHiddenNodes = totalHiddenNodes / runsCompleted; 
                averageEnabledGenes = totalEnabledGenes / runsCompleted; 

                if (neuronCounts.Count > 0)
                {
                    averageNeuronCount = neuronCounts.Average();
                    STDDEVNeuronCount = Math.Sqrt(neuronCounts.Select(neuronCount =>
                                                    Math.Pow(neuronCount - averageNeuronCount, 2))
                                                .Average());
                }
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
        
        public static double FitnessFunction(CPPNNEATGenome genome) {
            var result = (from num in Enumerable.Range(0, 4) 
                    select Math.Round(genome.Phenome.GetActivation(new double[] {num / 2, num % 2}), 
                                        OUTPUT_ACTIVATION_PRECISION)).ToArray();

            return Math.Pow(4 - result.Zip(CORRECT_RESULT, 
                                        (a, t) => Math.Abs(t - a))
                                      .Sum(), 2);
        }
    }
}
