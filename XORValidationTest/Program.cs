//#define DEBUG_GA
#define GENERATIONAL_GA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.IO;

namespace XORValidationTest
{
    public class Program
    {
#if DEBUG_GA
        private const int NUMBER_OF_RUNS = 1;
        private static int POPULATION_SIZE = 10;
#if GENERATIONAL_GA
        private static int MATING_LIMIT = 100;
#else 
        private static int MATING_LIMIT = 5000;
#endif

#else 
        private const int NUMBER_OF_RUNS = 100;
        private static int POPULATION_SIZE = 150;
        private static int MATING_LIMIT = -1;
#endif

        private static double OPTIMAL_SCORE = 16;
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;
        
#if GENERATIONAL_GA 
        private static double INTERSPECIES_MATING_RATE = 0.001;
        private static int MATING_EVENTS_PER_GENERATION = 1;
        private static int NO_INNOVATION_THRESHOLD = MATING_EVENTS_PER_GENERATION * 15;
        private static double ELITISM_RATE = 0.33;
#else
        private static int MATING_EVENTS_PER_GENERATION = 75;
        private static int NO_INNOVATION_THRESHOLD = MATING_EVENTS_PER_GENERATION * 15;
#endif

        private static readonly double[] CORRECT_RESULT = new double[] {0, 1, 1, 0};

        private static double WEIGHT_MUTATION_RATE = 0.8;
        private static double NEW_NEURON_RATE = 0.03;
        private static double NEW_LINK_RATE = 0.05;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 10;
        private static double MAX_WEIGHT = 20;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;
        private const double MATCHING_GENES_WEIGHT = 0.4;

        private const int OUTPUT_ACTIVATION_PRECISION = 2;
        private const int DEBUG_PRECISION = 8;

        private const string DEBUG_FILE = "debug.txt";
        private const double CROSSOVER_RATE = 0.75;
        
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

            var debugFile = new StreamWriter(new FileStream(DEBUG_FILE, FileMode.Create));

            Console.WriteLine(String.Format("0 out of {0} runs completed.", NUMBER_OF_RUNS));

            foreach(var i in Enumerable.Range(0, NUMBER_OF_RUNS)) 
            {
                //Initialise the GA.
                var testGA = new CPPNNEATGA(2, POPULATION_SIZE, FitnessFunction, 
                    new List<Func<double,double>>() {
                        CPPNActivationFunctions.SteepenedSigmoidActivationFunction
                    },
                    true);

                testGA.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
                testGA.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;

                testGA.WeightMutationRate = WEIGHT_MUTATION_RATE;
                testGA.NewNeuronRate = NEW_NEURON_RATE;
                testGA.NewLinkRate = NEW_LINK_RATE;
                
                testGA.DisableGeneRate = DISABLE_GENE_RATE;

                testGA.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
                testGA.MaxPerturbation = MAX_PERTURBATION;
                testGA.MaxWeight = MAX_WEIGHT;

                testGA.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
                testGA.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
                testGA.MatchingGenesWeight = MATCHING_GENES_WEIGHT;

#if GENERATIONAL_GA
                testGA.CrossoverRate = CROSSOVER_RATE;
                testGA.ElitismRate = ELITISM_RATE;
                testGA.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;
#endif

                testGA.Initialise();

                var matingEvents = 0;

                while (Math.Round(testGA.Best.Score, OUTPUT_ACTIVATION_PRECISION) < OPTIMAL_SCORE && !testGA.Failed)
                {
#if GENERATIONAL_GA
                    testGA.GenerationalIterate();
#else 
                    testGA.SteadyStateIterate();
#endif
                    if (++matingEvents == MATING_LIMIT)
                    {
                        break;
                    }

                    if(matingEvents % MATING_EVENTS_PER_GENERATION == 0) 
                    {
                        Console.Clear();
#if GENERATIONAL_GA
                        Console.WriteLine("Generational");
#else
                        Console.WriteLine("Steady-State");
#endif
                        Console.WriteLine();
                        Console.WriteLine(String.Format("{0} out of {1} runs completed.", i, NUMBER_OF_RUNS));

                        Console.Write(matingEvents / MATING_EVENTS_PER_GENERATION);
                        Console.WriteLine(" generations completed for current run.");
                        Console.WriteLine();
                        Console.WriteLine(String.Format("Current fitness: {0:f2}", testGA.Best.Score));
                        Console.WriteLine();
                        Console.Write("Average number of generations: ");
                        Console.WriteLine(averageMatingEvents / MATING_EVENTS_PER_GENERATION);
                        Console.Write("Average number of hidden nodes: ");
                        Console.WriteLine(averageHiddenNodes);
                        Console.Write("Average number of enabled genes: ");
                        Console.WriteLine(averageEnabledGenes);
                        Console.Write("Standard deviation of neuron count: ");
                        Console.WriteLine(STDDEVNeuronCount);
                        Console.WriteLine();
                        Console.Write("Number of failures: ");
                        Console.WriteLine(numberOfFailures);
                    }

#if DEBUG_GA
                    debugFile.Write("Iteration ");
                    debugFile.WriteLine(matingEvents.ToString());

                    debugFile.WriteLine();

                    debugFile.WriteLine(testGA.DebugInformation());
#endif
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

            debugFile.Close();
        }
        
        public static double FitnessFunction(CPPNNEATGenome genome) {
            var result = (from num in Enumerable.Range(0, 4) 
                    select genome.Phenome.GetActivation(new double[] {num / 2, num % 2})).ToArray();

            return Math.Pow(4 - result.Zip(CORRECT_RESULT, 
                                        (a, t) => Math.Abs(t - a))
                                      .Sum(), 2);
        }
    }
}
