using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    [TestFixture]
    public class CPPNNEATGeneCollectionTests
    {
        private CPPNNEATGeneCollection testCollection;

        private int BIAS_NEURON_INDEX = 0;
        private int OUTPUT_NEURON_INDEX = 1;
        private int HIDDEN_NEURON_INDEX = 4;
        private int SECOND_INPUT_NEURON_INDEX = 3;

        private int BIAS_TO_OUTPUT_INDEX = 0;
        private int FIRST_INPUT_TO_OUTPUT_INDEX = 1;
        private int SECOND_INPUT_TO_OUTPUT_INDEX = 2;

        private int BIAS_TO_HIDDEN_INDEX = 5;
        private int FIRST_INPUT_TO_HIDDEN_INDEX = 3;
        private int SECOND_INPUT_TO_HIDDEN_INDEX = 6;
        private int HIDDEN_TO_OUTPUT_INDEX = 4;

        private int OUTPUT_TO_HIDDEN_INDEX = 7;

        [SetUp]
        public void SetUp()
        {
            var testGA =  new CPPNNEATGA(CPPNNEATConstants.NUMBER_OF_INPUTS, 1, x => 0, 
                                            new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction });
            testGA.Initialise();

            var testGenome = testGA.Population[0];

            this.testCollection = testGenome.GeneCollection;

            this.testCollection.UpdateLinkGeneWeight(BIAS_TO_OUTPUT_INDEX, CPPNNEATConstants.BIAS);
            this.testCollection.UpdateLinkGeneWeight(FIRST_INPUT_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_1);
            this.testCollection.UpdateLinkGeneWeight(SECOND_INPUT_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_2);
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestInitialise(double input1, double input2)
        {
            testCollection.Update();
            Assert.AreEqual(CPPNNEATConstants.DefaultActivation(input1, input2),
                    testCollection.Phenome.GetActivation(new[] { input1, input2 }));
        }


        private void UpdateHiddenNeuronNetwork()
        {
            testCollection.CreateNeuronGene(FIRST_INPUT_TO_OUTPUT_INDEX);

            testCollection.TryCreateLinkGene(BIAS_NEURON_INDEX, HIDDEN_NEURON_INDEX);
            testCollection.TryCreateLinkGene(SECOND_INPUT_NEURON_INDEX, HIDDEN_NEURON_INDEX);

            testCollection.DisableLinkGene(BIAS_TO_OUTPUT_INDEX);
            testCollection.DisableLinkGene(SECOND_INPUT_TO_OUTPUT_INDEX);

            this.testCollection.UpdateLinkGeneWeight(BIAS_TO_HIDDEN_INDEX, CPPNNEATConstants.BIAS);
            this.testCollection.UpdateLinkGeneWeight(FIRST_INPUT_TO_HIDDEN_INDEX, CPPNNEATConstants.WEIGHT_1);
            this.testCollection.UpdateLinkGeneWeight(SECOND_INPUT_TO_HIDDEN_INDEX, CPPNNEATConstants.WEIGHT_2);
            this.testCollection.UpdateLinkGeneWeight(HIDDEN_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_3);
        }
        
        private static double HiddenNeuronActivation(double weight5Coefficient, double input1, double input2) 
        {
            return CPPNNEATConstants.OUTPUT_ACTIVATION_FUNCTION(
                CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION(CPPNNEATConstants.WEIGHT_3 + weight5Coefficient * CPPNNEATConstants.WEIGHT_5) *
                CPPNNEATConstants.WEIGHT_4 + input1 * CPPNNEATConstants.WEIGHT_1 + input2 * CPPNNEATConstants.WEIGHT_2);
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestHiddeNeuron(double input1, double input2)
        {
            UpdateHiddenNeuronNetwork();

            testCollection.Update();

            var network = testCollection.Phenome;

            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), 
                        network.GetActivation(new double[] { input1, input2 }));
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivationRecursive(double input1, double input2)
        {
            UpdateHiddenNeuronNetwork();

            testCollection.TryCreateLinkGene(OUTPUT_NEURON_INDEX, HIDDEN_NEURON_INDEX);

            testCollection.UpdateLinkGeneWeight(OUTPUT_TO_HIDDEN_INDEX, CPPNNEATConstants.WEIGHT_4);
            testCollection.Update();

            var network = testCollection.Phenome;

            var output1 = network.GetActivation(new double[] { input1, input2 });
            var output2 = network.GetActivation(new double[] { input1, input2 });

            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation2(input1, input2, output1), output2);
        }

        [TestCase(CPPNNEATConstants.NUMBER_OF_INPUTS + 1)]
        [TestCase(CPPNNEATConstants.NUMBER_OF_INPUTS - 1)]
        public void TestNumberOfInputs(int numberOfTestInputs)
        {
            Assert.Throws<ApplicationException>(
                delegate()
                {
                    testCollection.Update();
                    testCollection.Phenome.GetActivation((from i in Enumerable.Range(0, numberOfTestInputs) select 1.0).ToArray());
                }
            );
        }

        [TestCase]
        public void TestInputLinkException()
        {
            Assert.Throws<ApplicationException>(delegate() { testCollection.TryCreateLinkGene(BIAS_NEURON_INDEX, BIAS_NEURON_INDEX); });
            Assert.Throws<ApplicationException>(delegate() { testCollection.TryCreateLinkGene(BIAS_NEURON_INDEX, SECOND_INPUT_NEURON_INDEX); });
        }

        [TestCase]
        public void TestCannotCreateParallel()
        {
            var numberOfLinks = testCollection.LinkGenes.Count;

            Assert.AreEqual(false, testCollection.TryCreateLinkGene(BIAS_NEURON_INDEX, OUTPUT_NEURON_INDEX));
            Assert.AreEqual(numberOfLinks, testCollection.LinkGenes.Count);
        }

        [TestCase]
        public void TestTryCreateLinkGene()
        {
            UpdateHiddenNeuronNetwork();

            while (testCollection.TryCreateLinkGene())
            {
            }

            Assert.AreEqual(10, testCollection.LinkGenes.Count);
        }
    }
}
