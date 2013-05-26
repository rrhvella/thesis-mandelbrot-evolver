/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneticAlgorithms;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using ComplexCPPNNEAT;
using System.Numerics;
using MandelbrotCPPNNEAT;

namespace ComplexCPPNNEATSelection
{
    public class FractalSelection : Panel
    {
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 6.0;

        private static int NO_INNOVATION_THRESHOLD = Int32.MaxValue;

        private static int ITERATIONS_TO_CLEAR_LINK_CACHE = 1;

        private static double WEIGHT_MUTATION_RATE = 0.9;

        private static double NEW_NEURON_RATE = 0.12;

        private static double NEW_LINK_RATE = 0.24;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;

        private static double MAX_PERTURBATION = 0.01;

        private static double MAX_WEIGHT = 1.5;

        private const double EXCESS_GENES_WEIGHT = 1.0;

        private const double DISJOINT_GENES_WEIGHT = 1.0;

        private const double MATCHING_GENES_WEIGHT = 3.0;

        private const double FUNCTION_DIFFERENCE_WEIGHT = 1.0;

        private const double SURVIVAL_TRESHOLD = 0.3;

        private const double INTERSPECIES_MATING_RATE = 0.001;

        private const double CROSSOVER_RATE = 0.5;

        private const int VIEW_WIDTH = 50;

        private const int VIEW_HEIGHT = 50;

        private const int IMAGES_PER_ROW = 4;

        private const int POPULATION_SIZE = 16;

        private static readonly Complex VIEW_POSITION = new Complex(-2.2, -1.5);

        private const double VIEW_SCALE = 3;

        private const double MATE_BY_AVERAGING_RATE = 0.4;

        private List<FractalView> views;
        public IEnumerable<FractalView> Views
        {
            get
            {
                return views.AsReadOnly();
            }
        }

        private MandelbrotCPPNNEATGA ga;

        public FractalSelection()
        {
            views = new List<FractalView>();

            foreach (var i in Enumerable.Range(0, POPULATION_SIZE))
            {
                var fractalView = new FractalView();

                fractalView.ViewResolutionWidth = VIEW_WIDTH;
                fractalView.ViewResolutionHeight = VIEW_HEIGHT;

                views.Add(fractalView);

                Controls.Add(fractalView);
                fractalView.Show();
            }


            this.ga = new MandelbrotCPPNNEATGA(POPULATION_SIZE,
                        delegate(MandelbrotCPPNNEATGenome genome)
                        {
                            var pictureBox = views.Where(image => image.Genome == genome).FirstOrDefault();
                            return (pictureBox != null) ? pictureBox.Score : 0;
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
                        CPPNActivationFunctionFactories.ComplexLinearActivationFunctionFactory
                   );

            ga.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            ga.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;
            ga.IterationsToClearLinkCache = ITERATIONS_TO_CLEAR_LINK_CACHE;

            ga.WeightMutationRate = WEIGHT_MUTATION_RATE;
            ga.NewNeuronRate = NEW_NEURON_RATE;
            ga.NewLinkRate = NEW_LINK_RATE;

            ga.DisableGeneRate = DISABLE_GENE_RATE;

            ga.SurvivalTreshold = SURVIVAL_TRESHOLD;
            ga.InterSpeciesMatingRate = INTERSPECIES_MATING_RATE;

            ga.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;
            ga.MaxPerturbation = MAX_PERTURBATION;
            ga.MaxWeight = MAX_WEIGHT;

            ga.CrossoverRate = CROSSOVER_RATE;

            ga.ExcessGenesWeight = EXCESS_GENES_WEIGHT;
            ga.DisjointGenesWeight = DISJOINT_GENES_WEIGHT;
            ga.MatchingGenesWeight = MATCHING_GENES_WEIGHT;
            ga.FunctionDifferenceWeight = FUNCTION_DIFFERENCE_WEIGHT;

            ga.MateByAveragingRate = MATE_BY_AVERAGING_RATE;

            ga.ViewPosition = VIEW_POSITION;
            ga.ViewScale = VIEW_SCALE;

            ga.Initialise();

            LoadGenomesIntoImages();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            var fractalViewWidth = ClientSize.Width / IMAGES_PER_ROW;
            var fractalViewHeight = ClientSize.Height / (int)Math.Ceiling((double)POPULATION_SIZE / IMAGES_PER_ROW);

            foreach (var i in Enumerable.Range(0, views.Count))
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

        public int NumberOfGenerations
        {
            get
            {
                return ga.NumberOfGenerations;
            }
        }
    }
}
