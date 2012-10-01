﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.Extensions;
using System.Windows;

namespace NEATSpaces
{
    class Program: MapForm<CPPNNEATGA, CPPNNEATGenome, CPPNNEATGeneCollection, CPPNNetwork>
    {
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 1;

        private static int NO_INNOVATION_THRESHOLD = 1500;

        private static double WEIGHT_MUTATION_RATE = 1;
        private static double NEW_NEURON_RATE = 0.05;
        private static double NEW_LINK_RATE = 0.05;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 2.5;
        private static double MAX_WEIGHT = 5;

        private const double EXCESS_GENES_WEIGHT = 1;
        private const double DISJOINT_GENES_WEIGHT = 1;
        private const double MATCHING_GENES_WEIGHT = 1;
        private const double FUNCTION_DIFFERENCE_WEIGHT = 1;

        private const int NUMBER_OF_INPUTS = 2;
        private static readonly List<Func<double, double>> CANONICAL_FUNCTION_LIST = new List<Func<double, double>>
        {
            CPPNActivationFunctions.TanHActivationFunction,
            CPPNActivationFunctions.SinActivationFunction,
            CPPNActivationFunctions.ClippedLinearActivationFunction,
            CPPNActivationFunctions.GaussActivationFunction
        };

        private static readonly int NUMBER_OF_CRITICAL_POSITIONS = 2 + MapConstants.CHECKPOINTS.Count();

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override Map TransformPhenome(CPPNNetwork phenome)
        {
            var result = MapConstants.CreateMap();
            var maxDistance = MathExtensions.EuclideanDistance(new double[] { result.Width, result.Height });

            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    var current = new MapNode(x, y);
                    var input = new double[NUMBER_OF_INPUTS + NUMBER_OF_CRITICAL_POSITIONS];

                    input[0] = x / maxDistance;
                    input[1] = y / maxDistance;
                    input[2] = (current - result.StartNode).EuclideanDistance / maxDistance;
                    input[3] = (current - result.EndNode).EuclideanDistance / maxDistance;

                    var i = 4;
                    foreach (var checkpoint in result.Checkpoints)
                    {
                        input[i++] = (current - checkpoint).EuclideanDistance / maxDistance;
                    }

                    result[x, y] = phenome.GetActivation(input) >= 0;
                }
            }

            return result;
        }

        protected override CPPNNEATGA CreateGAListGA(int populationSize, Func<CPPNNEATGenome, double> scoreFunction)
        {
            var result = new CPPNNEATGA(NUMBER_OF_INPUTS + NUMBER_OF_CRITICAL_POSITIONS, 
                                        populationSize, scoreFunction, CANONICAL_FUNCTION_LIST, false);

            result.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            result.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;

            result.WeightMutationRate = WEIGHT_MUTATION_RATE;
            result.NewNeuronRate = NEW_NEURON_RATE;
            result.NewLinkRate = NEW_LINK_RATE;
            
            result.DisableGeneRate = DISABLE_GENE_RATE;

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
