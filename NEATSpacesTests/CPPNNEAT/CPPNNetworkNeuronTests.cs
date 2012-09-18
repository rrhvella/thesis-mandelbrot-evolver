using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    public class CPPNNetworkNeuronTests
    {
        public void TestClampedInput()
        {
            Assert.AreEqual(1, (new CPPNBiasNeuron()).Activation);
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivation(double input1, double input2)
        {
            var outputNeuron = new CPPNOutputNeuron(CPPNNEATConstants.OUTPUT_ACTIVATION_FUNCTION);
            CreateNeuronNetwork(input1, input2, outputNeuron);

            Assert.AreEqual(CPPNNEATConstants.DefaultActivation(input1, input2), outputNeuron.Activation);
        }

        [TestCase(1.0, 0.0)]
        [TestCase(0.0, -1.0)]
        [TestCase(-1.0, 1.0)]
        public void TestActivationRecursive(double input1, double input2)
        {
            var outputNeuron = new CPPNOutputNeuron(CPPNNEATConstants.OUTPUT_ACTIVATION_FUNCTION);
            var hiddenNeuron = new CPPNHiddenNeuron(CPPNNEATConstants.HIDDEN_ACTIVATION_FUNCTION);

            CreateNeuronNetwork(input1, input2, hiddenNeuron);

            outputNeuron.AddChild(hiddenNeuron, CPPNNEATConstants.WEIGHT_3);
            hiddenNeuron.AddChild(outputNeuron, CPPNNEATConstants.WEIGHT_4);

            var output1 = outputNeuron.Activation;
            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation1(input1, input2), output1);
            Assert.AreEqual(CPPNNEATConstants.RecursiveActivation2(input1, input2, output1), outputNeuron.Activation);
        }

        private void CreateNeuronNetwork(double input1, double input2, CPPNOutputNeuron outputNeuron)
        {
            var biasNeuron = new CPPNBiasNeuron();
            var inputNeuron1 = new CPPNInputNeuron();
            var inputNeuron2 = new CPPNInputNeuron();

            outputNeuron.AddChild(biasNeuron, CPPNNEATConstants.BIAS);
            outputNeuron.AddChild(inputNeuron1, CPPNNEATConstants.WEIGHT_1);
            outputNeuron.AddChild(inputNeuron2, CPPNNEATConstants.WEIGHT_2);

            inputNeuron1.SetInput(input1);
            inputNeuron2.SetInput(input2);
        }
    }
}
