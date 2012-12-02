using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;

namespace NEATSpacesLibrary.NEATSpaces
{
    public class FractalSelection: Form
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

        private int imagesPerRow = 4;
        private int imageWidth = 50;
        private int populationSize = 16;
        private int imageHeight = 50;
        private int zoomFactor = 2;

        private FractalView[] images;
        private CPPNNEATGA ga;

        public FractalSelection()
        {
            this.ClientSize = new Size(imagesPerRow * imageWidth * zoomFactor, 
                            (int)(Math.Ceiling(populationSize / (float)imagesPerRow) * imageHeight * zoomFactor));

            images = new FractalView[populationSize];

            foreach (var i in Enumerable.Range(0, populationSize))
            {
                var newPictureBox = new FractalView(zoomFactor);

                newPictureBox.Width = imageWidth * zoomFactor;
                newPictureBox.Height = imageHeight * zoomFactor;

                newPictureBox.Left = imageWidth * (i % imagesPerRow) * zoomFactor;
                newPictureBox.Top = imageWidth * (i / imagesPerRow) * zoomFactor;

                images[i] = newPictureBox;

                Controls.Add(newPictureBox);
                newPictureBox.Show();
            }

            this.KeyDown += new KeyEventHandler(ImageSelectionForm_KeyDown);

            this.ga = new CPPNNEATGA(NUMBER_OF_INPUTS, populationSize,
                                        delegate(CPPNNEATGenome genome)
                                        {
                                            var pictureBox = images.Where(image => image.Genome == genome).FirstOrDefault();
                                            return (pictureBox != null)? pictureBox.Score : 0;
                                        }, new List<Func<Func<Complex,Complex>>>  {
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

        private void ImageSelectionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ga.ForceUpdateGenomes();
                ga.GenerationalIterate();
                LoadGenomesIntoImages();
            }
        }

        private void LoadGenomesIntoImages()
        {
            var index = 0;

            foreach (var genome in ga.Population.OrderByDescending(genome => genome.Score))
            {
                images[index].Score = 0;
                images[index++].Genome = genome;
            }

            Refresh();
        }

        public static void Main(String[] args)
        {
            Application.Run(new FractalSelection());
        }
    }
}
