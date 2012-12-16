using System;
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
using MandelbrotCPPNNEAT;

namespace ComplexCPPNNEATSelection
{
    /// <summary>
    /// Maintains a collection of fractal views and maps them to the genomes in a genetic algorithm. 
    /// </summary>
    public class FractalSelection: Panel
    {
        /// <summary>
        /// Determines the point at which an individual is not part of a species, based on its distance to the species representative.
        /// </summary>
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 6.0;

        /// <summary>
        /// Determines the number of stagnant generations required for a species to be culled.
        /// </summary>
        private static int NO_INNOVATION_THRESHOLD = Int32.MaxValue;

        /// <summary>
        /// Determines the number of generations before the link cache is cleared.
        /// <seealso cref="BaseCPPNNEATGA:IterationsToClearLinkCache"/>
        /// </summary>
        private static int ITERATIONS_TO_CLEAR_LINK_CACHE = 1;

        /// <summary>
        /// Determines the rate at which weights are modified during mutation.
        /// </summary>
        private static double WEIGHT_MUTATION_RATE = 0.9;

        /// <summary>
        /// Determines the rate at which new neurons are added during mutation.
        /// </summary>
        private static double NEW_NEURON_RATE = 0.12;

        /// <summary>
        /// Determines the rate at which new links are added during mutation.
        /// </summary>
        private static double NEW_LINK_RATE = 0.24;

        /// <summary>
        /// Determines the rate at which mutated weights are perturbed as opposed to reinitialised.
        /// </summary>
        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        /// <summary>
        /// For a child genome, this determines the probability of a link gene being disabled if it is disabled for a parent 
        /// genome.
        /// </summary>
        private static double DISABLE_GENE_RATE = 0.75;

        /// <summary>
        /// The maximum magnitude of the value by which a weight is modified during perturbation.
        /// </summary>
        private static double MAX_PERTURBATION = 0.01;
        
        /// <summary>
        /// The maximum magnitude each weight is initialised to.
        /// </summary>
        private static double MAX_WEIGHT = 1.5;

        /// <summary>
        /// The weight of the number of excess genes on the compatibility distance. 
        /// </summary>
        private const double EXCESS_GENES_WEIGHT = 1.0;

        /// <summary>
        /// The weight of the number of disjoint genes on the compatibility distance. 
        /// </summary>
        private const double DISJOINT_GENES_WEIGHT = 1.0;

        /// <summary>
        /// The weight of the average difference in link weights on the compatibility distance. 
        /// </summary>
        private const double MATCHING_GENES_WEIGHT = 3.0;

        /// <summary>
        /// The weight of the average difference in function counts on the compatibility distance. 
        /// </summary>
        private const double FUNCTION_DIFFERENCE_WEIGHT = 1.0;

        /// <summary>
        /// The proportion  of a species population which is used for breeding.
        /// </summary>
        private const double SURVIVAL_TRESHOLD = 0.3;

        /// <summary>
        /// The rate at which individuals mate with individuals outside of their species.
        /// </summary>
        private const double INTERSPECIES_MATING_RATE = 0.001;
        
        /// <summary>
        /// The rate at which children are generated using crossover and mutation, instead of just mutation.
        /// </summary>
        private const double CROSSOVER_RATE = 0.5;

        /// <summary>
        /// The resolution width of the fractal views. Resolution here refers to the number of complex numbers sampled 
        /// within a view.
        /// </summary>
        private const int VIEW_WIDTH = 50;

        /// <summary>
        /// The resolution height of the fractal views. Resolution here refers to the number of complex numbers sampled 
        /// within a view.
        /// </summary>
        private const int VIEW_HEIGHT = 50;

        /// <summary>
        /// The maximum number of views placed in each row.
        /// </summary>
        private const int IMAGES_PER_ROW = 4;

        /// <summary>
        /// The population of the GA, which corresponds to the number of views.
        /// </summary>
        private const int POPULATION_SIZE = 16;

        /// <summary>
        /// The complex number at the top left corner of each view.
        /// </summary>
        private static readonly Complex VIEW_POSITION = new Complex(-2.2, -1.5);
        
        /// <summary>
        /// The distance, in the complex plane, between the two corners of each side of the view.
        /// </summary>
        private const double VIEW_SCALE = 3;

        /// <summary>
        /// The rate at which parent weights are averaged, as opposed to selecting one or the other.
        /// </summary>
        private const double MATE_BY_AVERAGING_RATE = 0.4;

        /// <summary>
        /// The fractal views displayed in this control.
        /// </summary>
        private List<FractalView> views;
        public IEnumerable<FractalView> Views 
        {
            get
            {
                return views.AsReadOnly();
            }
        }

        /// <summary>
        /// The GA on which the views are based.
        /// </summary>
        private MandelbrotCPPNNEATGA ga;

        public FractalSelection()
        {
            //Build the views.
            views = new List<FractalView>();

            foreach (var i in Enumerable.Range(0, POPULATION_SIZE))
            {
                var fractalView = new FractalView();

                fractalView.ViewWidth = VIEW_WIDTH;
                fractalView.ViewHeight = VIEW_HEIGHT;

                views.Add(fractalView);
                
                Controls.Add(fractalView);
                fractalView.Show();
            }

            //Build the GA.
            this.ga = new MandelbrotCPPNNEATGA(POPULATION_SIZE,
                                    delegate(MandelbrotCPPNNEATGenome genome)
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
            
            //Display the views.
            LoadGenomesIntoImages();
        }

        /// <summary>
        /// Handles the event fired when the control is resized.
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            //Resize the views to neatly fit into the new client window of the control.
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

        /// <summary>
        /// Load the next generation of the GA.
        /// </summary>
        public void NextGeneration()
        {
            ga.ForceUpdateGenomes();
            ga.GenerationalIterate();
            LoadGenomesIntoImages();
        }

        /// <summary>
        /// Place each genome in the population in its respective view, based on the order of the views and the descending order
        /// of the genome scores.
        /// </summary>
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

        /// <summary>
        /// The number of generations performed by the GA.
        /// </summary>
        public int NumberOfGenerations
        {
            get
            {
                return ga.NumberOfGenerations;
            }
        }
    }
}
