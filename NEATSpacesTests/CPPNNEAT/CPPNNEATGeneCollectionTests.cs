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

        private const int NUMBER_OF_INPUTS = 2;
        private const double BIAS = -0.5;
        private const double WEIGHT_1 = 0.5;
        private const double WEIGHT_2 = -0.5;
        private const double WEIGHT_3 = 2.0;
        private const double WEIGHT_4 = 1.5;
        private const double WEIGHT_5 = 0.5;

        [SetUp]
        public void Setup()
        {
            testCollection = new CPPNNEATGeneCollection();
            testCollection.Parent = new CPPNNEATGA(NUMBER_OF_INPUTS, 0, null, new List<Func<double[], double>>() { null });

            testCollection.LinkGenes[0].Weight = BIAS;
            testCollection.LinkGenes[1].Weight = WEIGHT_1;
            testCollection.LinkGenes[2].Weight = WEIGHT_2;
        }

        private static double DefaultActivation(double input1, double input2) 
        {
            return BIAS + WEIGHT_1 * input1 + WEIGHT_2 * input2;
        }

        private static double HiddenNeuronActivation(double weight5Coefficient, double input1, double input2) 
        {
            return (WEIGHT_3 + weight5Coefficient * WEIGHT_5) * WEIGHT_4 + input1 * WEIGHT_1 + input2 * WEIGHT_2;
        }

        public static IEnumerable<TestCaseData> InitialiseTestCases 
        {
            get 
            {
                Func<double[], TestCaseData> testDataFactory = delegate(double[] input) {
                    return new TestCaseData(input).Returns(DefaultActivation(input[0], input[1]));
                };

                yield return testDataFactory(new [] { 1.0, 0});
                yield return testDataFactory(new [] { 0, -1.0});
                yield return testDataFactory(new [] { -1.0, 1.0});
            }
        }

        [TestCaseSource(typeof(CPPNNEATGeneCollectionTests), "InitialiseTestCases")]
        public double TestInitialise(double[] input)
        {
            return testCollection.CreateNetwork().GetActivation(input);
        }

        [TestCase(Result=true)]
        public bool TestCreateNeuron()
        {
            testCollection.CreateNeuronGene(0);

            testCollection.LinkGenes[3].Weight = WEIGHT_3;
            testCollection.LinkGenes[4].Weight = WEIGHT_4;

            var input1 = 0.5;
            var input2 = 0.5;

            return testCollection.CreateNetwork().GetActivation(new double[] { input1, input2 }) == 
                                    HiddenNeuronActivation(0, input1, input2);
        }

        [TestCase(Result=true)]
        public bool TestCreateLinkGeneFeedForward()
        {
            testCollection.CreateNeuronGene(0);
            testCollection.CreateLinkGene(1, 4);

            testCollection.LinkGenes[3].Weight = WEIGHT_3;
            testCollection.LinkGenes[4].Weight = WEIGHT_4;
            testCollection.LinkGenes[5].Weight = WEIGHT_5;

            var input1 = 0.5;
            var input2 = 0.5;

            return testCollection.CreateNetwork().GetActivation(new double[] { input1, input2 }) == 
                                    HiddenNeuronActivation(input1, input1, input2);
        }

        [TestCase(Result=true)]
        public bool TestCreateLinkGeneRecursive()
        {
            testCollection.CreateLinkGene(3, 2);

            testCollection.LinkGenes[3].Weight = WEIGHT_3;

            var input1 = 0.5;
            var input2 = 0.5;

            var network = testCollection.CreateNetwork();
            var output1 = network.GetActivation(new double[] { input1, input2 });
            var output2 = network.GetActivation(new double[] { input1, input2 });

            return output1 == DefaultActivation(input1, input2) &&
                output2 == DefaultActivation(input1, input2) + output1 * WEIGHT_3;
        }

        [TestCase(Result = true)]
        public bool TestDisableGene()
        {
            testCollection.CreateLinkGene(3, 2);

            testCollection.LinkGenes[3].Weight = WEIGHT_3;
            testCollection.LinkGenes[3].Enabled = false;

            var input1 = 0.5;
            var input2 = 0.5;

            var network = testCollection.CreateNetwork();

            return network.GetActivation(new double[] { input1, input2 }) == DefaultActivation(input1, input2) &&
                network.GetActivation(new double[] { input1, input2 }) == DefaultActivation(input1, input2);
        }

        [TestCase(ExpectedException = typeof(ApplicationException))]
        public void TestInputLinkException()
        {
            testCollection.CreateLinkGene(0, 1);
        }

    }
}
