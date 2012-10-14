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
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;

        private static int NO_INNOVATION_THRESHOLD = 1000;

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

        private static readonly List<Func<double, double>> CANONICAL_FUNCTION_LIST = new List<Func<double, double>>
        {
            CPPNActivationFunctions.TanHActivationFunction,
            CPPNActivationFunctions.SinActivationFunction,
            CPPNActivationFunctions.ClippedLinearActivationFunction,
            CPPNActivationFunctions.GaussActivationFunction
        };

        private static readonly int NUMBER_OF_CRITICAL_POSITIONS = 2 + MapConstants.CHECKPOINTS.Count();

        private double maxDistance;
        private double[][] euclideanDistanceCache;

        public Program() 
        {
            maxDistance = 30;
            euclideanDistanceCache = new double[MapConstants.MAP_SIZE * MapConstants.MAP_SIZE][];

            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    var current = new MapNode(x, y);
                    var cacheRecord = new double[NUMBER_OF_CRITICAL_POSITIONS];

                    cacheRecord[0] = (current - MapConstants.START_NODE).EuclideanDistance / maxDistance;
                    cacheRecord[1] = (current - MapConstants.END_NODE).EuclideanDistance / maxDistance;

                    var i = 2;
                    foreach (var checkpoint in MapConstants.CHECKPOINTS)
                    {
                        cacheRecord[i++] = (current - checkpoint).EuclideanDistance / maxDistance;
                    }

                    euclideanDistanceCache[y * MapConstants.MAP_SIZE + x] = cacheRecord;
                }
            }
        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override Map TransformPhenome(CPPNNetwork phenome)
        {
            var result = MapConstants.CreateMap();

            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    var current = new MapNode(x, y);
                    var cacheRecord = euclideanDistanceCache[y * MapConstants.MAP_SIZE + x];

                    var input = new double[NUMBER_OF_INPUTS + NUMBER_OF_CRITICAL_POSITIONS];

                    input[0] = x / maxDistance;
                    input[1] = y / maxDistance;

                    foreach(var i in Enumerable.Range(0, NUMBER_OF_CRITICAL_POSITIONS))
                    {
                        input[i + NUMBER_OF_INPUTS] = cacheRecord[i];
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
