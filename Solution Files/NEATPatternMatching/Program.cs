using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATPatternMatching
{
    public class Program: PatternMatchingForm<CPPNNEATGA, CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork>
    {
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;

        private const double INPUT_DIVISOR = 30;

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

        private const int NUMBER_OF_INPUTS = 1;

        private static readonly Func<double, double> OUTPUT_ACTIVATION_FUNCTION = CPPNActivationFunctions.StepActivationFunction; 

        private static readonly List<Func<double, double>> CANONICAL_FUNCTION_LIST = new List<Func<double, double>>
        {
            CPPNActivationFunctions.TanHActivationFunction,
            CPPNActivationFunctions.SinActivationFunction,
            CPPNActivationFunctions.ClippedLinearActivationFunction,
            CPPNActivationFunctions.GaussActivationFunction
        };


        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override int[] TransformPhenome(CPPNNetwork phenome)
        {
            phenome.Reset();

            var targetArrayLength = PatternMatchingConstants.TARGET_PATTERN.Length;
            return Enumerable.Range(0, targetArrayLength)
                    .Select(i => (int)phenome.GetActivation(new double[] { i / (double)targetArrayLength }))
                    .ToArray();
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
    }
}
