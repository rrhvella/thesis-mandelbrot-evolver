using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    public class CPPNNEATGenomeTests
    {
        private const int NUMBER_OF_MATING_EVENTS = 10;
        private const int NUMBER_OF_GENERATIONAL_MATING_EVENTS = 10;
        private const int NUMBER_OF_GENERATIONAL_RUNS = 10;

        private readonly double[] INPUT = new[] { 0.0, 0.0 };
        private readonly double[,] XOR_TRUTH_TABLE = new double[,] { {0, 0, 0}, {0, 1, 1}, {1, 0, 1}, {1, 1, 0} };

        private int NUMBER_OF_INPUTS = 2;
        private int NUMBER_OF_GENOMES = 2;
        private const int POPULATION = 10;

        private static int NO_INNOVATION_THRESHOLD = 15;
        private static double COMPATIBILITY_DISTANCE_THRESHOLD = 3.0;
        private static double MAX_PERTURBATION = 2.5;
        private const double MATCHING_GENES_WEIGHT = 0.4;
        private static double ELITISM_RATE = 0.2;

        private static double WEIGHT_MUTATION_RATE = 0.8;
        private static double NEW_NEURON_RATE = 0.03;
        private static double NEW_LINK_RATE = 0.05;

        private static double WEIGHT_PERTUBATION_RATE = 0.9;

        private static double DISABLE_GENE_RATE = 0.75;
        private static double MAX_WEIGHT = 5;

        private const double EXCESS_GENES_WEIGHT = 1.0;
        private const double DISJOINT_GENES_WEIGHT = 1.0;

        private static double INTERSPECIES_MATING_RATE = 0.001;

        private const double CROSSOVER_RATE = 0.75;
        private const double MATE_BY_AVERAGING_RATE = 0.4;
        private const int INPUT_START = 2;

        [TestCase]
        public void TestMatingAndMutationStability()
        {
            MatingAndMutationStabilityTest(false, delegate(CPPNNEATGenome testChild)
            {
                var inputsWithBias = (NUMBER_OF_INPUTS + 1);
                var childNeuronsWithoutInputs = testChild.GeneCollection.NeuronGenes.Count - inputsWithBias;

                return inputsWithBias * childNeuronsWithoutInputs +
                               childNeuronsWithoutInputs * childNeuronsWithoutInputs;
            },
            null);
        }

        [TestCase]
        public void TestOrderingOfInputNeurons()
        {
            MatingAndMutationStabilityTest(false, 
            null, delegate(CPPNNEATGenome testChild)
            {
                foreach(var i in Enumerable.Range(INPUT_START, NUMBER_OF_INPUTS)) {
                    Assert.AreEqual(i, testChild.GeneCollection.NeuronGenes[i].InnovationNumber);
                }
            });
        }

        [TestCase]
        public void TestMatingAndMutationStabilityFeedForward()
        {
            MatingAndMutationStabilityTest(true, delegate(CPPNNEATGenome testChild)
            {
                var neuronLevelCount = (from neuron in testChild.GeneCollection.NeuronGenes
                                        group neuron by neuron.Level into neuronsByLevel
                                        orderby neuronsByLevel.Key
                                        select neuronsByLevel.Count()).ToArray();

                var totalExpectedEdges = 0;
                foreach (var nIndex in Enumerable.Range(0, neuronLevelCount.Length - 1))
                {
                    foreach (var n2Index in Enumerable.Range(nIndex + 1, neuronLevelCount.Length - nIndex - 1))
                    {
                        totalExpectedEdges += neuronLevelCount[nIndex] * neuronLevelCount[n2Index];
                    }
                }

                return totalExpectedEdges;
            }, delegate(CPPNNEATGenome testChild)
            {
                testChild.UpdatePhenome();
                Assert.AreEqual(testChild.Phenome.GetActivation(INPUT), testChild.Phenome.GetActivation(INPUT));
            });
        }

        private void MatingAndMutationStabilityTest(bool feedForward, 
                                Func<CPPNNEATGenome, int> getExpectedEdges,
                                Action<CPPNNEATGenome> otherTests)
        {
            var ga = new CPPNNEATGA(NUMBER_OF_INPUTS, NUMBER_OF_GENOMES, x => 0.0, 
                        new List<Func<double, double>>() { CPPNActivationFunctions.LinearActivationFunction },
                        feedForward);

            ga.Initialise();

            ga.NewLinkRate = NEW_LINK_RATE;
            ga.NewNeuronRate = NEW_NEURON_RATE;
            ga.MaxPerturbation = MAX_PERTURBATION;
            ga.MaxWeight = MAX_WEIGHT;
            ga.WeightMutationRate = WEIGHT_MUTATION_RATE;
            ga.WeightPertubationRate = WEIGHT_PERTUBATION_RATE;

            var parent = ga.Population[0];
            var partner = ga.Population[1];

            foreach(var i in Enumerable.Range(0, NUMBER_OF_MATING_EVENTS)) {
                
                Assert.DoesNotThrow(delegate()
                {
                    var children = parent.Crossover(partner);

                    //Perform tests on child 2.
                    var testChild = children[1];

                    var childGenesCount = testChild.GeneCollection.LinkGenes.Count;
                    var parentGenesCount = parent.GeneCollection.LinkGenes.Count;
                    var partnerGenesCount = partner.GeneCollection.LinkGenes.Count;
                    
                    while (testChild.GeneCollection.TryCreateLinkGene())
                    {
                    }

                    if (getExpectedEdges != null)
                    {
                        Assert.AreEqual(getExpectedEdges((CPPNNEATGenome)testChild),
                                testChild.GeneCollection.LinkGenes.Count());
                        Assert.AreEqual(0,
                                testChild.GeneCollection.LinkGenes.Where(link => !link.Enabled).Count());
                    }

                    if (otherTests != null)
                    {
                        otherTests((CPPNNEATGenome)children[0]);
                    }

                    //Mutate parents for next generation.
                    parent = partner;
                    partner = (CPPNNEATGenome)children[0];

                    partner.Species = parent.Species;

                    parent.Mutate();
                    partner.Mutate();

                    parent.UpdatePhenome();
                    parent.Phenome.GetActivation(new double[] { 0, 0 });

                    partner.UpdatePhenome();
                    partner.Phenome.GetActivation(new double[] { 0, 0 });
                });
            }
        }


        [TestCase(new int[] {0, 1}, new int[] {0, 2}, 1, 1, 0, 0, 1)]
        [TestCase(new int[] {0, 1, 2}, new int[] {0, 1, 2}, 3, 0, 0, 0, 0)]
        [TestCase(new int[] {0, 2, 1}, new int[] {0, 1, 2}, 3, 0, 0, 0, 0)]
        [TestCase(new int[] {0, 2}, new int[] {0, 1, 2}, 2, 0, 1, 0, 0)]
        [TestCase(new int[] {0, 1, 2}, new int[] {0, 1}, 2, 0, 0, 1, 0)]
        [TestCase(new int[] {0, 1, 2, 4}, new int[] {2, 4}, 2, 2, 0, 0, 0)]
        public void TestDifferenceAnalysis(int[] ids1, int[] ids2, int matches, 
                                        int disjointFirst, int disjointSecond, 
                                        int excessFirst, int excessSecond)
        {
            var ga = new CPPNNEATGA(1, 2, x => 0, new List<Func<double, double>>() { x => 0 }, false);
            ga.Initialise();

            var geneCollection1 = ga.Population[0].GeneCollection;
            var geneCollection2 = ga.Population[1].GeneCollection;

            var biasPlusInputs = 1 + ga.NumberOfInputs;

            var neuron = new CPPNNEATNeuronGene(0, 1, CPPNNeuronType.Hidden, x => 0);

            foreach (var i in ids1)
            {
                geneCollection1.TryAddLinkGene(new CPPNNEATLinkGene(i + biasPlusInputs, neuron, neuron, 0));
            }

            foreach (var i in ids2)
            {
                geneCollection2.TryAddLinkGene(new CPPNNEATLinkGene(i + biasPlusInputs, neuron, neuron, 0));
            }

            var differenceAnalysis = new CPPNNEATGenome.DifferenceAnalysis(geneCollection1, geneCollection2);

            Assert.AreEqual(matches + biasPlusInputs, differenceAnalysis.Matches.Count);
            Assert.AreEqual(disjointFirst, differenceAnalysis.FirstCollection.Disjoint.Count);
            Assert.AreEqual(disjointSecond, differenceAnalysis.SecondCollection.Disjoint.Count);
            Assert.AreEqual(excessFirst, differenceAnalysis.FirstCollection.Excess.Count);
            Assert.AreEqual(excessSecond, differenceAnalysis.SecondCollection.Excess.Count);
        }

        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(2, 3, 4)]
        public void TestFunctionAnalysis(int function1Count, int function2Count, int function3Count)
        {
            var func1 = new Func<double,double>(x => x);
            var func2 = new Func<double,double>(x => x);
            var func3 = new Func<double,double>(x => x);
            
            var ga = new CPPNNEATGA(CPPNNEATConstants.NUMBER_OF_INPUTS, 1, x => 0, 
                                new List<Func<double, double>>() { func1, func2, func3 }, false);
            ga.Initialise();

            var genome = ga.Population[0];

            var geneCollection = ga.Population[0].GeneCollection;
            geneCollection.UpdateNeuronActivationFunction(CPPNNEATConstants.OUTPUT_NEURON_INDEX, null);

            int neuronIndex = CPPNNEATConstants.HIDDEN_NEURON_INDEX;

            foreach (var i in Enumerable.Range(0, function1Count))
            {
                geneCollection.TryCreateNeuronGene();
                geneCollection.UpdateNeuronActivationFunction(neuronIndex++, func1);
            }

            foreach (var i in Enumerable.Range(0, function2Count))
            {
                geneCollection.TryCreateNeuronGene();
                geneCollection.UpdateNeuronActivationFunction(neuronIndex++, func2);
            }

            foreach (var i in Enumerable.Range(0, function3Count))
            {
                geneCollection.TryCreateNeuronGene();
                geneCollection.UpdateNeuronActivationFunction(neuronIndex++, func3);
            }

            var functionAnalysis = genome.GetFunctionAnalysis();
            
            Assert.AreEqual(function1Count, functionAnalysis[func1]);
            Assert.AreEqual(function2Count, functionAnalysis[func2]);
            Assert.AreEqual(function3Count, functionAnalysis[func3]);
        }

        [TestCase]
        public void TestGenomeCopying()
        {
            var testGA = new CPPNNEATGA(NUMBER_OF_INPUTS, 1, x => 0, new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction },
                                        false);
            testGA.Initialise();

            var testGenome = testGA.Population[0];

            var testGenomeCopy = testGenome.Copy();

            testGenome.UpdatePhenome();
            testGenomeCopy.UpdatePhenome();

            Assert.AreEqual(testGenome.GeneCollection.Phenome.GetActivation(INPUT),
                        testGenomeCopy.GeneCollection.Phenome.GetActivation(INPUT));

            Assert.AreEqual(testGenome.GeneCollection.Phenome.GetActivation(INPUT),
                        testGenomeCopy.GeneCollection.Phenome.GetActivation(INPUT));

            Assert.AreNotSame(testGenome, testGenomeCopy);
            Assert.AreNotSame(testGenome.GeneCollection, testGenomeCopy.GeneCollection);
            Assert.AreNotSame(testGenome.Phenome, testGenomeCopy.Phenome);

            Assert.AreEqual(testGenomeCopy.GeneCollection.LinkGenes.Count * 2,
                        testGenomeCopy.GeneCollection.LinkGenes.Union(testGenome.GeneCollection.LinkGenes).Count());

        }

        private void GenerationalStabilityTest(Action<CPPNNEATGA> test, bool feedForwardOnly)
        {
            var testGA = new CPPNNEATGA(NUMBER_OF_INPUTS, POPULATION, genome => TestScoreFunction(genome, !feedForwardOnly), 
                                        new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction },
                                        feedForwardOnly);

            testGA.CompatibilityDistanceThreshold = COMPATIBILITY_DISTANCE_THRESHOLD;
            testGA.NoInnovationThreshold = NO_INNOVATION_THRESHOLD;

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

            foreach (var i in Enumerable.Range(0, NUMBER_OF_GENERATIONAL_RUNS))
            {
                foreach (var j in Enumerable.Range(0, NUMBER_OF_GENERATIONAL_MATING_EVENTS))
                {
                    testGA.GenerationalIterate();
                    test(testGA);
                }
            }
        }

        [TestCase]
        public void TestGenerationalPopulationStability()
        {
            GenerationalStabilityTest(delegate(CPPNNEATGA testGA)
            {
                Assert.AreEqual(POPULATION, testGA.Population.Count);
            }, false);
        }

        [TestCase]
        public void TestGenerationalPhenomeActivationStability()
        {
            GenerationalStabilityTest(delegate(CPPNNEATGA testGA)
            {
                Assert.AreEqual(testGA.Best.Score, TestScoreFunction(testGA.Best, true));
            }, false);
        }

        [TestCase]
        public void TestGenerationalFeedForwardOnlyStability()
        {
            GenerationalStabilityTest(delegate(CPPNNEATGA testGA)
            {
                Assert.AreEqual(testGA.Best.Score, TestScoreFunction(testGA.Best, false));
            }, true);
        }

        private double TestScoreFunction(CPPNNEATGenome genome, bool reset)
        {
            if (reset)
            {
                genome.Phenome.Reset();
            }

            var totalError = 0.0;

            foreach (var i in Enumerable.Range(0, 4))
            {
                totalError += Math.Abs(XOR_TRUTH_TABLE[i, 2] -
                                genome.Phenome.GetActivation(new double[] { XOR_TRUTH_TABLE[i, 0], XOR_TRUTH_TABLE[i, 1] })); 
            }

            totalError = 4 - totalError;

            return totalError * totalError;
        } 
    }
}
