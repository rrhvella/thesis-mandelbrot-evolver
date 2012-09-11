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

        [SetUp]
        public void SetUp()
        {
            this.testCollection = new CPPNNEATGeneCollection();
            this.testCollection.Parent = new CPPNNEATGA(CPPNNEATConstants.NUMBER_OF_INPUTS, 0, null, 
                                            new List<Func<double, double>>() { CPPNActivationFunctions.TanHActivationFunction });

            this.testCollection.Initialise();

            this.testCollection.LinkGenes[0].Weight = CPPNNEATConstants.BIAS;
            this.testCollection.LinkGenes[1].Weight = CPPNNEATConstants.WEIGHT_1;
            this.testCollection.LinkGenes[2].Weight = CPPNNEATConstants.WEIGHT_2;
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
            testCollection.CreateNeuronGene(1);
            testCollection.NeuronGenes[4].ActivationFunction = CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION;

            testCollection.CreateLinkGene(0, 4);
            testCollection.CreateLinkGene(2, 4);

            testCollection.LinkGenes[0].Enabled = false;
            testCollection.LinkGenes[2].Enabled = false;

            testCollection.LinkGenes[5].Weight = CPPNNEATConstants.BIAS;
            testCollection.LinkGenes[3].Weight = CPPNNEATConstants.WEIGHT_1;
            testCollection.LinkGenes[6].Weight = CPPNNEATConstants.WEIGHT_2;
            testCollection.LinkGenes[4].Weight = CPPNNEATConstants.WEIGHT_3;
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
        public void TestActivationRecursive(double input1, double input2)
        {
            UpdateHiddenNeuronNetwork();

            testCollection.CreateLinkGene(3, 4);

            testCollection.LinkGenes[7].Weight = CPPNNEATConstants.WEIGHT_4;
            testCollection.Update();

            var network = testCollection.Phenome;

            var output1 = network.GetActivation(new double[] { input1, input2 });
            var output2 = network.GetActivation(new double[] { input1, input2 });

            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), output1);
            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation2(input1, input2, output1), output2);
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivationParallel(double input1, double input2)
        {
            UpdateHiddenNeuronNetwork();

            testCollection.CreateLinkGene(4, 3);

            testCollection.LinkGenes[7].Weight = CPPNNEATConstants.WEIGHT_4;
            testCollection.Update();

            var network = testCollection.Phenome;

            Assert.AreEqual(CPPNNEATConstants.ParallelActivation(input1, input2), 
                        network.GetActivation(new double[] { input1, input2 }));
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
        public void TestCreateNeuron()
        {
            testCollection.CreateNeuronGene(0);
            testCollection.NeuronGenes[4].ActivationFunction = CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION;

            testCollection.LinkGenes[3].Weight = CPPNNEATConstants.WEIGHT_3;
            testCollection.LinkGenes[4].Weight = CPPNNEATConstants.WEIGHT_4;

            var input1 = 0.5;
            var input2 = 0.5;

            Assert.AreEqual(false, testCollection.LinkGenes[0].Enabled);
            Assert.AreEqual(CPPNNeuronType.Hidden, testCollection.NeuronGenes[4].Type);

            testCollection.Update();
            Assert.AreEqual(HiddenNeuronActivation(0, input1, input2), 
                    testCollection.Phenome.GetActivation(new double[] { input1, input2 }));
        }

        [TestCase]
        public void TestCreateLinkGene()
        {
            testCollection.CreateNeuronGene(0);
            testCollection.NeuronGenes[4].ActivationFunction = CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION;

            testCollection.CreateLinkGene(1, 4);

            testCollection.LinkGenes[3].Weight = CPPNNEATConstants.WEIGHT_3;
            testCollection.LinkGenes[4].Weight = CPPNNEATConstants.WEIGHT_4;
            testCollection.LinkGenes[5].Weight = CPPNNEATConstants.WEIGHT_5;

            var input1 = 0.5;
            var input2 = 0.5;

            testCollection.Update();
            Assert.AreEqual(HiddenNeuronActivation(input1, input1, input2), 
                        testCollection.Phenome.GetActivation(new double[] { input1, input2 }));
        }

        [TestCase]
        public void TestDisableGene()
        {
            testCollection.CreateLinkGene(2, 3);

            testCollection.LinkGenes[3].Weight = CPPNNEATConstants.WEIGHT_3;
            testCollection.LinkGenes[3].Enabled = false;

            var input1 = 0.5;
            var input2 = 0.5;

            testCollection.Update();
            var network = testCollection.Phenome;

            Assert.AreEqual(CPPNNEATConstants.DefaultActivation(input1, input2), network.GetActivation(new double[] { input1, input2 }));
        }

        [TestCase]
        public void TestInputLinkException()
        {
            Assert.Throws<ApplicationException>(delegate() { testCollection.CreateLinkGene(0, 0); });
            Assert.Throws<ApplicationException>(delegate() { testCollection.CreateLinkGene(0, 1); });
        }
    }
}
