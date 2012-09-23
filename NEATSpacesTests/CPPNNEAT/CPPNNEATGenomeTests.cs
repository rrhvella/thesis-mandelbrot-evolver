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
        private readonly double[] INPUT = new double[] { 0.0, 0.0 };

        private int NUMBER_OF_INPUTS = 2;
        private int NUMBER_OF_GENOMES = 2;
        private double NEW_LINK_RATE = 1;
        private double NEW_NEURON_RATE = 1;
        private double MAX_PERTURBATION_RATE = 2;
        private double MAX_WEIGHT = 5;
        private double WEIGHT_MUTATION_RATE = 1;
        private double WEIGHT_PERTURBATION_RATE = 1;

        [TestCase]
        public void TestMatingAndMutationStability()
        {
            var ga = new CPPNNEATGA(NUMBER_OF_INPUTS, NUMBER_OF_GENOMES, x => 0.0, 
                        new List<Func<double, double>>() { CPPNActivationFunctions.LinearActivationFunction },
                        false);

            ga.Initialise();

            ga.NewLinkRate = NEW_LINK_RATE;
            ga.NewNeuronRate = NEW_NEURON_RATE;
            ga.MaxPerturbation = MAX_PERTURBATION_RATE;
            ga.MaxWeight = MAX_WEIGHT;
            ga.WeightMutationRate = WEIGHT_MUTATION_RATE;
            ga.WeightPertubationRate = WEIGHT_PERTURBATION_RATE;

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

                    Assert.IsTrue(childGenesCount == parentGenesCount ||
                                childGenesCount == partnerGenesCount); 

                    Assert.AreEqual(childGenesCount + parentGenesCount + partnerGenesCount,
                                testChild.GeneCollection.LinkGenes.Union(
                                    partner.GeneCollection.LinkGenes.Union(
                                        parent.GeneCollection.LinkGenes)).Count());

                    while (testChild.GeneCollection.TryCreateLinkGene())
                    {
                    }

                    var inputsWithBias = (ga.NumberOfInputs + 1);
                    var childNeuronsWithoutInputs = testChild.GeneCollection.NeuronGenes.Count - inputsWithBias;

                    Assert.AreEqual(inputsWithBias * childNeuronsWithoutInputs +
                                   childNeuronsWithoutInputs * childNeuronsWithoutInputs,
                                   testChild.GeneCollection.LinkGenes.Count);

                    Assert.AreEqual(0, testChild.GeneCollection.LinkGenes.Where(link => !link.Enabled).Count());

                    //Mutate parents for next generation.
                    parent = partner;
                    partner = (CPPNNEATGenome)children[0];

                    partner.Species = parent.Species;

                    parent.Mutate();
                    partner.Mutate();

                    parent.UpdatePhenome();
                    parent.Phenome.GetActivation(INPUT);

                    partner.UpdatePhenome();
                    partner.Phenome.GetActivation(INPUT);
                });
            }
        }

        [TestCase]
        public void TestMatingAndMutationStabilityFeedForward()
        {
            var ga = new CPPNNEATGA(NUMBER_OF_INPUTS, NUMBER_OF_GENOMES, x => 0.0, 
                        new List<Func<double, double>>() { CPPNActivationFunctions.LinearActivationFunction },
                        true);

            ga.Initialise();

            ga.NewLinkRate = NEW_LINK_RATE;
            ga.NewNeuronRate = NEW_NEURON_RATE;
            ga.MaxPerturbation = MAX_PERTURBATION_RATE;
            ga.MaxWeight = MAX_WEIGHT;
            ga.WeightMutationRate = WEIGHT_MUTATION_RATE;
            ga.WeightPertubationRate = WEIGHT_PERTURBATION_RATE;

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
                    
                    Assert.AreEqual(totalExpectedEdges, testChild.GeneCollection.LinkGenes.Count());
                    Assert.AreEqual(0, testChild.GeneCollection.LinkGenes.Where(link => !link.Enabled).Count());

                    //Use first child to test nn.
                    children[0].UpdatePhenome();
                    Assert.AreEqual(children[0].Phenome.GetActivation(INPUT), children[0].Phenome.GetActivation(INPUT));

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
            var biasPlusInputs = 1 + ga.NumberOfInputs;

            var neuron = new CPPNNEATNeuronGene(0, 1, CPPNNeuronType.Hidden, x => 0);

            foreach (var i in ids1)
            {
                geneCollection1.TryAddLinkGene(new CPPNNEATLinkGene(i + biasPlusInputs, neuron, neuron, 0));
            }

            var geneCollection2 = ga.Population[1].GeneCollection;

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

        /*
        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(2, 3, 4)]
        public void TestFunctionAnalysis(int function1Count, int function2Count, int function3Count)
        {
            var func1 = new Func<double,double>(x => x);
            var func2 = new Func<double,double>(x => x);
            var func3 = new Func<double,double>(x => x);
            
            var ga = new CPPNNEATGA(1, 1, null, new List<Func<double, double>>() {func1, func2, func3});
            ga.Initialise();

            var genome = ga.Population[0];

            var geneCollection = ga.Population[0].GeneCollection;
            geneCollection.UpdateNeuronActivationFunction(2, null);

            foreach (var i in Enumerable.Range(0, function1Count))
            {
                geneCollection.AddNeuronGene(new CPPNNEATNeuronGene(CPPNNeuronType.Hidden, func1));
            }

            foreach (var i in Enumerable.Range(0, function2Count))
            {
                geneCollection.AddNeuronGene(new CPPNNEATNeuronGene(CPPNNeuronType.Hidden, func2));
            }

            foreach (var i in Enumerable.Range(0, function3Count))
            {
                geneCollection.AddNeuronGene(new CPPNNEATNeuronGene(CPPNNeuronType.Hidden, func3));
            }

            var functionAnalysis = genome.GetFunctionAnalysis();
            
            Assert.AreEqual(function1Count, functionAnalysis[func1]);
            Assert.AreEqual(function2Count, functionAnalysis[func2]);
            Assert.AreEqual(function3Count, functionAnalysis[func3]);
        }
        */
    }
}
