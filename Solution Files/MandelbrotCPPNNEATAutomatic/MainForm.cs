using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using MandelbrotCPPNNEAT;
using System.Drawing;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;

namespace MandelbrotCPPNNEATAutomatic
{
    class MainForm: GAExperiment<MandelbrotCPPNNEATGA, MandelbrotCPPNNEATGenome, CPPNNEATGeneCollection,
                                MandelbrotCPPNNEATPhenome>
    {
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 4.0;

        private static int NO_INNOVATION_THRESHOLD = 15000;
        private static int ITERATIONS_TO_CLEAR_LINK_CACHE = 100;

        private static double WEIGHT_MUTATION_RATE = 0.9;
        private static double NEW_NEURON_RATE = 0.12;
        private static double NEW_LINK_RATE = 0.16;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 0.01;
        private static double MAX_WEIGHT = 1.5;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;
        private const double MATCHING_GENES_WEIGHT = 3.0;
        private const double FUNCTION_DIFFERENCE_WEIGHT = 0.0;
        private const double VIEW_POSITION_DISTANCE_COEFFICIENT = 1.0;
        private const double VIEW_SCALE_DISTANCE_COEFFICIENT= 2.0;

        private const double ELITISM_RATE = 0.5;
        private const double INTERSPECIES_MATING_RATE = 0.001;
        
        private const double CROSSOVER_RATE = 0.5;

        private const int NUMBER_OF_INPUTS = 2;

        private const int VIEW_WIDTH = 100;
        private const int VIEW_HEIGHT = 100;

        private const int IMAGES_PER_ROW = 4;
        private const int POPULATION_SIZE = 100;

        private const int ESCAPE = 50;

        private static readonly Complex VIEW_POSITION = new Complex(-2.2, -1.5);
        private const double VIEW_SCALE = 3;

        private const int TEST_WIDTH = 30;
        private const int TEST_HEIGHT = 30;

        private const int TEST_ESCAPE = 50;

        private const int NUMBER_OF_GENERATIONS = 100;
        private const int NUMBER_OF_RUNS = 40;
        private const int MATING_EVENTS_PER_GENERATION = 200;
        private const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 100;

        public static void Main(string[] args)
        {
            MainForm program = new MainForm();
            program.Run();
        }

        public MainForm():base(NUMBER_OF_GENERATIONS,
                            NUMBER_OF_RUNS,
                            MATING_EVENTS_PER_GENERATION,
                            NUMBER_OF_TICKS_TO_UPDATE_IMAGE,
                            POPULATION_SIZE)
        {
        }

        protected override MandelbrotCPPNNEATGA 
            CreateGAListGA(int populationSize, Func<MandelbrotCPPNNEATGenome, double> scoreFunction)
        {
            var ga = new MandelbrotCPPNNEATGA(NUMBER_OF_INPUTS, POPULATION_SIZE,
                                                scoreFunction, new List<Func<CPPNNEATActivationFunction>>  {
                                                CPPNActivationFunctionFactories.ComplexLinearActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexExponentialActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexLogarithmicActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexTanHActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexEulerActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexPolynomialActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexGaussianActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexSinActivationFunctionFactory,
                                                },
                                                CPPNActivationFunctionFactories.ComplexLinearActivationFunctionFactory);

            ga.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            ga.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;
            ga.IterationsToClearLinkCache = ITERATIONS_TO_CLEAR_LINK_CACHE;

            ga.WeightMutationRate = WEIGHT_MUTATION_RATE;
            ga.NewNeuronRate = NEW_NEURON_RATE;
            ga.NewLinkRate = NEW_LINK_RATE;

            ga.DisableGeneRate = DISABLE_GENE_RATE;

            ga.ElitismRate = ELITISM_RATE;
            ga.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;

            ga.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
            ga.MaxPerturbation = MAX_PERTURBATION;
            ga.MaxWeight = MAX_WEIGHT;

            ga.CrossoverRate = CROSSOVER_RATE;

            ga.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
            ga.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
            ga.MatchingGenesWeight = MATCHING_GENES_WEIGHT;
            ga.FunctionDifferenceWeight = FUNCTION_DIFFERENCE_WEIGHT;

            ga.ViewPosition = VIEW_POSITION;
            ga.ViewScale = VIEW_SCALE;

            return ga;
        }

        protected override Image GenerateImage(MandelbrotCPPNNEATPhenome phenome)
        {
            return phenome.GetImage(VIEW_WIDTH, VIEW_HEIGHT, ESCAPE);
        }

        protected override double ScoreFunction(MandelbrotCPPNNEATGenome genome)
        {
            return genome.Phenome.GetVarianceBasedFitness(TEST_WIDTH, TEST_HEIGHT, TEST_ESCAPE) + 1;
        }
    }
}
