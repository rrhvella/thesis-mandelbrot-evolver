﻿using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;

namespace ComplexCPPNNEATSelection
{
    public class FractalSelection: Panel
    {
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;

        private static int NO_INNOVATION_THRESHOLD = 1000000;
        private static int ITERATIONS_TO_CLEAR_LINK_CACHE = 1;

        private static double WEIGHT_MUTATION_RATE = 0.9;
        private static double NEW_NEURON_RATE = 0.04;
        private static double NEW_LINK_RATE = 0.08;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 0.01;
        private static double MAX_WEIGHT = 1.5;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;
        private const double MATCHING_GENES_WEIGHT = 3.0;
        private const double FUNCTION_DIFFERENCE_WEIGHT = 0.0;

        private const double ELITISM_RATE = 0.3;
        private const double INTERSPECIES_MATING_RATE = 0.001;
        
        private const double CROSSOVER_RATE = 0.5;

        private const int NUMBER_OF_INPUTS = 2;

        private const int VIEW_WIDTH = 50;
        private const int VIEW_HEIGHT = 50;

        private const int IMAGES_PER_ROW = 4;
        private const int POPULATION_SIZE = 16;

        private List<FractalView> views;
        public IEnumerable<FractalView> Views 
        {
            get
            {
                return views.AsReadOnly();
            }
        }

        private CPPNNEATGA ga;

        private Complex viewPosition;
        public Complex ViewPosition
        {
            get
            {
                return viewPosition;
            }
            set
            {
                viewPosition = value;

                foreach (var fractalView in views)
                {
                    fractalView.ViewPosition = viewPosition;
                }
            }
        }

        private double viewSize;
        public double ViewSize
        {
            get
            {
                return viewSize;
            }
            set
            {
                viewSize = value;

                foreach (var fractalView in views)
                {
                    fractalView.ViewSize = viewSize;
                }
            }
        }

        public FractalSelection()
        {
            views = new List<FractalView>();

            foreach (var i in Enumerable.Range(0, POPULATION_SIZE))
            {
                var fractalView = new FractalView(VIEW_WIDTH, VIEW_HEIGHT);

                fractalView.ViewSize = ViewSize;

                views.Add(fractalView);
                
                Controls.Add(fractalView);
                fractalView.Show();
            }

            this.ga = new CPPNNEATGA(NUMBER_OF_INPUTS, POPULATION_SIZE,
                                        delegate(CPPNNEATGenome genome)
                                        {
                                            var pictureBox = views.Where(image => image.Genome == genome).FirstOrDefault();
                                            return (pictureBox != null)? pictureBox.Score : 0;
                                        }, new List<Func<CPPNNEATActivationFunction>>  {
                                                CPPNActivationFunctionFactories.ComplexLinearActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexExponentialActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexLogarithmicActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexTanHActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexEulerActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexPolynomialActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexGaussianActivationFunctionFactory,
                                                CPPNActivationFunctionFactories.ComplexSinActivationFunctionFactory,
                                                },
                                                CPPNActivationFunctionFactories.ComplexLinearActivationFunctionFactory,
                                        true);

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

            ga.Initialise();
            LoadGenomesIntoImages();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            var fractalViewWidth = ClientSize.Width / IMAGES_PER_ROW;
            var fractalViewHeight = ClientSize.Height / (int)Math.Ceiling((double)POPULATION_SIZE / IMAGES_PER_ROW);

            foreach(var i in Enumerable.Range(0, views.Count)) 
            {
                var view = views[i];

                view.Width = fractalViewWidth;
                view.Height = fractalViewHeight;

                view.Left = view.Width * (i % IMAGES_PER_ROW);
                view.Top = view.Height * (i / IMAGES_PER_ROW);
            }
        }

        public void NextGeneration()
        {
            ga.ForceUpdateGenomes();
            ga.GenerationalIterate();
            LoadGenomesIntoImages();
        }

        private void LoadGenomesIntoImages()
        {
            var index = 0;

            foreach (var genome in ga.Population.OrderByDescending(genome => genome.Score))
            {
                views[index].Score = 0;
                views[index++].Genome = genome;
            }

            Refresh();
        }
    }
}