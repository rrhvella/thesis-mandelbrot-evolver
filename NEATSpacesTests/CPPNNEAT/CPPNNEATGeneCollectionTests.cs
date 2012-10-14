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
        private static readonly double[] INPUT = new double[] { 0.5, 0.5 };

        [SetUp]
        public void SetUp()
        {
            var testGA =  new CPPNNEATGA(CPPNNEATConstants.NUMBER_OF_INPUTS, 1, x => 0, 
                                            new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction },
                                            false);
            testGA.Initialise();

            var testGenome = testGA.Population[0];

            this.testCollection = testGenome.GeneCollection;

            UpdateBaseLinks(testCollection);
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


        private void UpdateHiddenNeuronNetwork(CPPNNEATGeneCollection testCollection)
        {
            testCollection.CreateNeuronGene(CPPNNEATConstants.FIRST_INPUT_TO_OUTPUT_INDEX);

            testCollection.TryCreateLinkGene(CPPNNEATConstants.BIAS_NEURON_INDEX, CPPNNEATConstants.HIDDEN_NEURON_INDEX);
            testCollection.TryCreateLinkGene(CPPNNEATConstants.SECOND_INPUT_NEURON_INDEX, CPPNNEATConstants.HIDDEN_NEURON_INDEX);

            testCollection.DisableLinkGene(CPPNNEATConstants.BIAS_TO_OUTPUT_INDEX);
            testCollection.DisableLinkGene(CPPNNEATConstants.SECOND_INPUT_TO_OUTPUT_INDEX);

            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.BIAS_TO_HIDDEN_INDEX, CPPNNEATConstants.BIAS);
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.FIRST_INPUT_TO_HIDDEN_INDEX, CPPNNEATConstants.WEIGHT_1);
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.SECOND_INPUT_TO_HIDDEN_INDEX, CPPNNEATConstants.WEIGHT_2);
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.HIDDEN_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_3);
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
            UpdateHiddenNeuronNetwork(this.testCollection);

            testCollection.Update();

            var network = testCollection.Phenome;

            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), 
                        network.GetActivation(new double[] { input1, input2 }));
            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), 
                        network.GetActivation(new double[] { input1, input2 }));
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivationRecursive(double input1, double input2)
        {
            UpdateHiddenNeuronNetwork(this.testCollection);

            testCollection.TryCreateLinkGene(CPPNNEATConstants.OUTPUT_NEURON_INDEX, 
                                            CPPNNEATConstants.HIDDEN_NEURON_INDEX);

            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.OUTPUT_TO_HIDDEN_INDEX, 
                                            CPPNNEATConstants.WEIGHT_4);
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
            Assert.Throws<ApplicationException>(delegate() { 
                    testCollection.TryCreateLinkGene(CPPNNEATConstants.BIAS_NEURON_INDEX, 
                                                        CPPNNEATConstants.BIAS_NEURON_INDEX); 
            });

            Assert.Throws<ApplicationException>(delegate() { 
                    testCollection.TryCreateLinkGene(CPPNNEATConstants.BIAS_NEURON_INDEX, 
                                                        CPPNNEATConstants.SECOND_INPUT_NEURON_INDEX); 
            });
        }

        [TestCase]
        public void TestFeedForwardRecursiveLinkException()
        {
            testCollection = GetFeedForwardNetwork();

            Assert.Throws<ApplicationException>(delegate() { 
                    testCollection.TryCreateLinkGene(CPPNNEATConstants.HIDDEN_NEURON_INDEX, 
                                                        CPPNNEATConstants.HIDDEN_NEURON_INDEX); 
            });

            Assert.Throws<ApplicationException>(delegate() { 
                    testCollection.TryCreateLinkGene(CPPNNEATConstants.OUTPUT_NEURON_INDEX, 
                                                        CPPNNEATConstants.OUTPUT_NEURON_INDEX); 
            });

            Assert.Throws<ApplicationException>(delegate() { 
                    testCollection.TryCreateLinkGene(CPPNNEATConstants.OUTPUT_NEURON_INDEX, 
                                                        CPPNNEATConstants.HIDDEN_NEURON_INDEX); 
            });
        }


        [TestCase]
        public void TestCannotCreateParallel()
        {
            var numberOfLinks = testCollection.LinkGenes.Count;

            Assert.AreEqual(false, testCollection.TryCreateLinkGene(CPPNNEATConstants.BIAS_NEURON_INDEX, CPPNNEATConstants.OUTPUT_NEURON_INDEX));
            Assert.AreEqual(numberOfLinks, testCollection.LinkGenes.Count);
        }

        [TestCase]
        public void TestTryCreateLinkGene()
        {
            UpdateHiddenNeuronNetwork(this.testCollection);

            while (testCollection.TryCreateLinkGene())
            {
            }

            Assert.AreEqual(10, testCollection.LinkGenes.Count);
        }

        [TestCase]
        public void TestTryCreateLinkGeneFeedForwardOnly()
        {
            this.testCollection = GetFeedForwardNetwork();

            while (testCollection.TryCreateLinkGene())
            {
                testCollection.Update();

                Assert.AreEqual(testCollection.Phenome.GetActivation(INPUT),
                                testCollection.Phenome.GetActivation(INPUT));
            }

            Assert.AreEqual(7, testCollection.LinkGenes.Count);
        }

        [TestCase]
        public void TestTryCreateLinkGeneMultiLayerFeedForwardOnly()
        {
            this.testCollection = GetFeedForwardNetwork();
            testCollection.CreateNeuronGene(CPPNNEATConstants.HIDDEN_TO_OUTPUT_INDEX);

            while (testCollection.TryCreateLinkGene())
            {
                testCollection.Update();

                Assert.AreEqual(testCollection.Phenome.GetActivation(INPUT),
                                testCollection.Phenome.GetActivation(INPUT));
            }

            Assert.AreEqual(12, testCollection.LinkGenes.Count);
        }

        private CPPNNEATGeneCollection GetFeedForwardNetwork()
        {
            var testGA =  new CPPNNEATGA(CPPNNEATConstants.NUMBER_OF_INPUTS, 1, x => 0, 
                                            new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction },
                                            true);
            testGA.Initialise();

            var testGenome = testGA.Population[0];

            var testCollection = testGenome.GeneCollection;

            UpdateBaseLinks(testCollection);
            UpdateHiddenNeuronNetwork(testCollection);

            return testCollection;
        }

        private void UpdateBaseLinks(CPPNNEATGeneCollection testCollection)
        {
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.BIAS_TO_OUTPUT_INDEX, CPPNNEATConstants.BIAS);
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.FIRST_INPUT_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_1);
            testCollection.UpdateLinkGeneWeight(CPPNNEATConstants.SECOND_INPUT_TO_OUTPUT_INDEX, CPPNNEATConstants.WEIGHT_2);
        }

        [TestCase]
        public void TestTryCreateLinkGeneAfterDisableLink()
        {
            UpdateHiddenNeuronNetwork(this.testCollection);

            while (testCollection.TryCreateLinkGene())
            {
            }

            Assert.IsTrue(testCollection.TryDisableLinkGene());
            Assert.IsTrue(testCollection.TryCreateLinkGene());
        }
    }
}
