using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.CPPNNEAT;

namespace HyperNEATValidationTest
{
    public class Program: GAExperiment<CPPNNEATGA, CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork>
    {
        public const int TRIAL_SIZE = 11;
        public const int DISPLAY_SIZE = 55;

        public const int NUMBER_OF_GENERATIONS = 300;
        public const int NUMBER_OF_REPLICANTS = 20;
        public const int MATING_EVENTS_PER_GENERATION = 1;
        public const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 100;

        public const int POPULATION_SIZE = 100;

        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;

        private const double INPUT_DIVISOR = 100;

        private static int NO_INNOVATION_THRESHOLD = 1000;
        private static int ITERATIONS_TO_CLEAR_LINK_CACHE = 20;

        private static double WEIGHT_MUTATION_RATE = 0.9;
        private static double NEW_NEURON_RATE = 0.1;
        private static double NEW_LINK_RATE = 0.1;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 1.8;
        private static double MAX_WEIGHT = MAX_PERTURBATION;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;
        private const double MATCHING_GENES_WEIGHT = 4.0;
        private const double FUNCTION_DIFFERENCE_WEIGHT = 1.0;

        private const double ELITISM_RATE = 0.3;
        private const double INTERSPECIES_MATING_RATE = 0.001;

        private const int NUMBER_OF_INPUTS = 2;

        private const int NUMBER_OF_TRIALS = 75;

        private static readonly Func<double, double> OUTPUT_ACTIVATION_FUNCTION = CPPNActivationFunctions.LinearActivationFunction; 

        private static readonly List<Func<double, double>> CANONICAL_FUNCTION_LIST = new List<Func<double, double>>
        {
            CPPNActivationFunctions.TanHActivationFunction,
            CPPNActivationFunctions.SinActivationFunction,
            CPPNActivationFunctions.ClippedLinearActivationFunction,
            CPPNActivationFunctions.GaussActivationFunction
        };

        public Program() : base(NUMBER_OF_GENERATIONS, NUMBER_OF_REPLICANTS, MATING_EVENTS_PER_GENERATION, 
                            NUMBER_OF_TICKS_TO_UPDATE_IMAGE, POPULATION_SIZE, true)
        {
        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override CPPNNEATGA CreateGAListGA(int populationSize, Func<CPPNNEATGenome, double> scoreFunction)
        {
            var result = new CPPNNEATGA(NUMBER_OF_INPUTS, populationSize, 
                                        scoreFunction, CANONICAL_FUNCTION_LIST, OUTPUT_ACTIVATION_FUNCTION, false);

            result.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            result.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;
            result.IterationsToClearLinkCache = ITERATIONS_TO_CLEAR_LINK_CACHE;

            result.WeightMutationRate = WEIGHT_MUTATION_RATE;
            result.NewNeuronRate = NEW_NEURON_RATE;
            result.NewLinkRate = NEW_LINK_RATE;
            
            result.DisableGeneRate = DISABLE_GENE_RATE;

            result.ElitismRate = ELITISM_RATE;
            result.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;

            result.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
            result.MaxPerturbation = MAX_PERTURBATION;
            result.MaxWeight = MAX_WEIGHT;

            result.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
            result.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
            result.MatchingGenesWeight = MATCHING_GENES_WEIGHT;
            result.FunctionDifferenceWeight = FUNCTION_DIFFERENCE_WEIGHT;

            return result;
        }

        protected override Image GenerateImage(CPPNNetwork phenome)
        {
            throw new NotImplementedException();
        }

        protected override double ScoreFunction(CPPNNEATGenome genome)
        {
            var phenome = genome.Phenome;

            foreach (var trialIndex in Enumerable.Range(0, NUMBER_OF_TRIALS))
            {
                var inputRecord = GetInputRecord(TRIAL_SIZE);
                var largeBoxCenter = inputRecord.LargeBoxCenter;


            }
        }

        private IEnumerator<double[]> TransformPhenome(CPPNNetwork network, double[] input)
        {
            return new HyperNEATSubstrate(network, substrateSize, substrateSize).
                    GetOutput(input);
        }
    }
}
