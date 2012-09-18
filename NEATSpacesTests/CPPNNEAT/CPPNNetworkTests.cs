using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    public class CPPNNetworkTests
    {
        private CPPNOutputNeuron outputNeuron;
        private CPPNBiasNeuron biasNeuron;
        private CPPNInputNeuron inputNeuron1;
        private CPPNInputNeuron inputNeuron2;
        private CPPNHiddenNeuron hiddenNeuron;

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivation(double input1, double input2)
        {
            var network = CreateNeuronNetwork();

            network.AddLink(biasNeuron, outputNeuron, CPPNNEATConstants.BIAS);
            network.AddLink(inputNeuron1, outputNeuron, CPPNNEATConstants.WEIGHT_1);
            network.AddLink(inputNeuron2, outputNeuron, CPPNNEATConstants.WEIGHT_2);

            Assert.AreEqual(CPPNNEATConstants.DefaultActivation(input1, input2), network.GetActivation(new[] { input1, input2 }));
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivationRecursive(double input1, double input2)
        {
            var network = CreateHiddenNeuronNetwork();
            network.AddLink(outputNeuron, hiddenNeuron, CPPNNEATConstants.WEIGHT_4);

            var output1 = network.GetActivation(new double[] { input1, input2 });

            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), output1);
            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation2(input1, input2, output1),
                    network.GetActivation(new double[] { input1, input2 }));
        }

        private CPPNNetwork CreateHiddenNeuronNetwork()
        {
            var network = CreateNeuronNetwork();

            hiddenNeuron = new CPPNHiddenNeuron(CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION);
            network.AddNeuron(hiddenNeuron);

            network.AddLink(biasNeuron, hiddenNeuron, CPPNNEATConstants.BIAS);
            network.AddLink(inputNeuron1, hiddenNeuron, CPPNNEATConstants.WEIGHT_1);
            network.AddLink(inputNeuron2, hiddenNeuron, CPPNNEATConstants.WEIGHT_2);

            network.AddLink(hiddenNeuron, outputNeuron, CPPNNEATConstants.WEIGHT_3);

            return network;
        }

        private CPPNNetwork CreateNeuronNetwork()
        {
            outputNeuron = new CPPNOutputNeuron(CPPNNEATConstants.OUTPUT_ACTIVATION_FUNCTION);
            biasNeuron = new CPPNBiasNeuron();

            inputNeuron1 = new CPPNInputNeuron();
            inputNeuron2 = new CPPNInputNeuron();

            var network = new CPPNNetwork();

            network.AddNeuron(outputNeuron);
            network.AddNeuron(biasNeuron);
            network.AddNeuron(inputNeuron1);
            network.AddNeuron(inputNeuron2);

            return network;
        }
    }
}
