//#define DEBUG_GA

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
        private static int MATING_LIMIT = 100;

#else 
        private const int NUMBER_OF_RUNS = 100;
        private static int POPULATION_SIZE = 150;
        private static int MATING_LIMIT = -1;
#endif

        
        private const int MATING_EVENTS_PER_GENERATION = 1;
        private static int NO_INNOVATION_THRESHOLD = MATING_EVENTS_PER_GENERATION * 15;
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;
        private static double MAX_PERTURBATION = 2.5;
        private const double MATCHING_GENES_WEIGHT = 0.4;
        private static double ELITISM_RATE = 0.2;

        private const int ITERATIONS_TO_CLEAR_LINK_CACHE = MATING_EVENTS_PER_GENERATION;
        private static readonly double[] CORRECT_RESULT = new double[] {0, 1, 1, 0};

        private static double WEIGHT_MUTATION_RATE = 0.8;
        private static double NEW_NEURON_RATE = 0.03;
        private static double NEW_LINK_RATE = 0.05;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;
        private static double MAX_WEIGHT = MAX_PERTURBATION;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;

        private static double INTERSPECIES_MATING_RATE = 0.001;

        private const string DEBUG_FILE = "debug.txt";
        private const double CROSSOVER_RATE = 0.75;
        private const double MATE_BY_AVERAGING_RATE = 0.4;

        private const int NUMBER_OF_INPUTS = 2;

        private static List<int> Generations;
        private static double AverageGenerations;
        private static double STDDEVGenerations;

        private static List<int> EnabledLinkCounts;
        private static double AverageEnabledLinkCount;
        private static double STDDEVEnabledLinkCount;

        private static List<int> NeuronCounts;
        private static double AverageNeuronCount;
        private static double STDDEVNeuronCount;

        private static int NumberOfFailures;
        
        public static void Main(string[] args)
        {
            NumberOfFailures = 0;

            Generations = new List<int>();

            AverageGenerations = 0.0;
            STDDEVGenerations = 0.0;

            EnabledLinkCounts = new List<int>();

            AverageEnabledLinkCount = 0.0;
            STDDEVEnabledLinkCount = 0.0;

            NeuronCounts = new List<int>();

            AverageNeuronCount = 0.0;
            STDDEVNeuronCount = 0.0;

            var debugFile = new StreamWriter(new FileStream(DEBUG_FILE, FileMode.Create));

            Console.WriteLine(String.Format("0 out of {0} runs completed.", NUMBER_OF_RUNS));

            foreach(var i in Enumerable.Range(0, NUMBER_OF_RUNS)) 
            {
                //Initialise the GA.
                var testGA = new CPPNNEATGA(NUMBER_OF_INPUTS, POPULATION_SIZE, FitnessFunction, 
                    new List<Func<double,double>>() {
                        CPPNActivationFunctions.SteepenedSigmoidActivationFunction,
                        CPPNActivationFunctions.GaussActivationFunction,
                        CPPNActivationFunctions.LinearActivationFunction,
                        CPPNActivationFunctions.SinActivationFunction,
                    },
                    CPPNActivationFunctions.SteepenedSigmoidActivationFunction,
                    false);

                testGA.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
                testGA.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;
                testGA.IterationsToClearLinkCache = ITERATIONS_TO_CLEAR_LINK_CACHE;

                testGA.WeightMutationRate = WEIGHT_MUTATION_RATE;
                testGA.NewNeuronRate = NEW_NEURON_RATE;
                testGA.NewLinkRate = NEW_LINK_RATE;
                
                testGA.DisableGeneRate = DISABLE_GENE_RATE;
                testGA.MateByAveragingRate = MATE_BY_AVERAGING_RATE;

                testGA.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
                testGA.MaxPerturbation = MAX_PERTURBATION;
                testGA.MaxWeight = MAX_WEIGHT;

                testGA.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
                testGA.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
                testGA.MatchingGenesWeight = MATCHING_GENES_WEIGHT;

                testGA.CrossoverRate = CROSSOVER_RATE;
                testGA.ElitismRate = ELITISM_RATE;
                testGA.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;

                testGA.Initialise();

                var matingEvents = 0;

                while (!testGA.Failed && !OptimalNetworkFound(testGA.Best))
                {
                    testGA.GenerationalIterate();

                    if (++matingEvents == MATING_LIMIT)
                    {
                        break;
                    }

                    if(matingEvents % MATING_EVENTS_PER_GENERATION == 0) 
                    {
                        PrintInfo(i, matingEvents, testGA);
                    }

#if DEBUG_GA
                    debugFile.Write("Iteration ");
                    debugFile.WriteLine(matingEvents.ToString());

                    debugFile.WriteLine();

                    debugFile.WriteLine(testGA.DebugInformation());
#endif
                }
                
                var best = testGA.Best;

                if (testGA.Failed)
                {
                    NumberOfFailures++;
                }
                else
                {
                    Generations.Add(matingEvents); 
                    EnabledLinkCounts.Add((from gene in best.GeneCollection.LinkGenes 
                                          where gene.Enabled 
                                          select gene).Count()); 
                    NeuronCounts.Add(best.GeneCollection.Phenome.NeuronCount);
                }

                if (i >= NumberOfFailures)
                {
                    var nMinus1 = (i - NumberOfFailures);

                    AverageNeuronCount = NeuronCounts.Average();
                    STDDEVNeuronCount = Math.Sqrt(NeuronCounts.Select(neuronCount =>
                                                    Math.Pow(neuronCount - AverageNeuronCount, 2))
                                                .Sum() / nMinus1);

                    AverageEnabledLinkCount = EnabledLinkCounts.Average();
                    STDDEVEnabledLinkCount = Math.Sqrt(EnabledLinkCounts.Select(enabledLinkCount =>
                                                    Math.Pow(enabledLinkCount - AverageEnabledLinkCount, 2))
                                                .Sum() / nMinus1);

                    AverageGenerations = Generations.Average() / MATING_EVENTS_PER_GENERATION;
                    STDDEVGenerations = Math.Sqrt(Generations.Select(generation =>
                                                    Math.Pow(generation / MATING_EVENTS_PER_GENERATION 
                                                                - AverageGenerations, 2))
                                                .Sum() / nMinus1);
                }

                PrintInfo(i + 1, matingEvents, testGA);
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            debugFile.Close();
        }

        private static void PrintInfo(int generationIndex, int matingEventsCompleted, CPPNNEATGA ga)
        {
            Console.Clear();
            Console.WriteLine("Generational");
            Console.WriteLine();
            Console.WriteLine(String.Format("{0} out of {1} runs completed.", generationIndex, NUMBER_OF_RUNS));

            Console.Write(matingEventsCompleted / MATING_EVENTS_PER_GENERATION);
            Console.WriteLine(" generations completed for current run.");
            Console.WriteLine();
            Console.WriteLine(String.Format("Current fitness: {0:f2}", ga.Best.Score));
            Console.WriteLine();
            Console.Write("Average number of generations: ");
            Console.WriteLine(AverageGenerations);
            Console.Write("Standard deviation of generations: ");
            Console.WriteLine(STDDEVGenerations);
            Console.WriteLine();
            Console.Write("Average number of nodes: ");
            Console.WriteLine(AverageNeuronCount);
            Console.Write("Standard deviation of neuron count: ");
            Console.WriteLine(STDDEVNeuronCount);
            Console.WriteLine();
            Console.Write("Average number of enabled genes: ");
            Console.WriteLine(AverageEnabledLinkCount);
            Console.Write("Standard deviation of enabled genes: ");
            Console.WriteLine(STDDEVEnabledLinkCount);
            Console.WriteLine();
            Console.Write("Number of failures: ");
            Console.WriteLine(NumberOfFailures);
        }

        private static bool OptimalNetworkFound(CPPNNEATGenome genome)
        {
            var truthTable = GetTruthTable(genome);

            return truthTable.Zip(CORRECT_RESULT, (a, t) => Math.Round(a) == t).All(result => result);
        }
        
        public static double FitnessFunction(CPPNNEATGenome genome) {
            var truthTable = GetTruthTable(genome);

            var result = Math.Pow(4 - truthTable.Zip(CORRECT_RESULT, 
                                        (a, t) => Math.Abs(t - a))
                                      .Sum(), 2);

            return result;
        }

        private static double[] GetTruthTable(CPPNNEATGenome genome)
        {
            genome.Phenome.Reset();
            return (from num in Enumerable.Range(0, 4)
                    select genome.Phenome.GetActivation(new double[] { num / 2, num % 2 })).ToArray();
        }
    }
}
