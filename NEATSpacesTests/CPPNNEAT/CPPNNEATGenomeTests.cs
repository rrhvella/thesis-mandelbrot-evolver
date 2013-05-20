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
        private readonly double[] INPUT = new[] { 0.0, 0.0 };
        private int NUMBER_OF_INPUTS = 2;

        [TestCase(new int[] { 0, 1 }, new int[] { 0, 2 }, 1, 1, 0, 0, 1)]
        [TestCase(new int[] { 0, 1, 2 }, new int[] { 0, 1, 2 }, 3, 0, 0, 0, 0)]
        [TestCase(new int[] { 0, 2, 1 }, new int[] { 0, 1, 2 }, 3, 0, 0, 0, 0)]
        [TestCase(new int[] { 0, 2 }, new int[] { 0, 1, 2 }, 2, 0, 1, 0, 0)]
        [TestCase(new int[] { 0, 1, 2 }, new int[] { 0, 1 }, 2, 0, 0, 1, 0)]
        [TestCase(new int[] { 0, 1, 2, 4 }, new int[] { 2, 4 }, 2, 2, 0, 0, 0)]
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
            var func1 = new Func<double, double>(x => x);
            var func2 = new Func<double, double>(x => x);
            var func3 = new Func<double, double>(x => x);

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
    }
}
