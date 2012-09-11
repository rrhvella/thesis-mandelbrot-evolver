using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    public class CPPNNEATGATests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TestConstructor(int numberOfInputs)
        {
            var newGA = new CPPNNEATGA(numberOfInputs, 0, null, new List<Func<double, double>>() { CPPNActivationFunctions.LinearActivationFunction });
            Console.WriteLine(newGA.DefaultNeuronGenes);
            var groupedNeurons = (from neuron in newGA.DefaultNeuronGenes 
                                 group neuron by neuron.Type into neuronsByType
                                 select neuronsByType).ToDictionary(group => group.Key, group => group.Count());


            Assert.AreEqual(1, groupedNeurons[CPPNNeuronType.Bias]);
            Assert.AreEqual(numberOfInputs, groupedNeurons[CPPNNeuronType.Input]);
            Assert.IsFalse(groupedNeurons.ContainsKey(CPPNNeuronType.Hidden));
            Assert.AreEqual(1, groupedNeurons[CPPNNeuronType.Output]);

            Assert.AreEqual(newGA.DefaultLinkGenes
                                .Where(linkGene => linkGene.To.Type == CPPNNeuronType.Output).Count(), 
                            1 + numberOfInputs);

            Assert.AreEqual(newGA.DefaultNeuronGenes.Where(neuron => neuron.Type == CPPNNeuronType.Output).First().ActivationFunction,
                        CPPNActivationFunctions.LinearActivationFunction);

            var groupedLinks = (from link in newGA.DefaultLinkGenes 
                                 group link by link.From.Type into linksByFromType
                                 select linksByFromType).ToDictionary(group => group.Key, group => group.Count());


            Assert.AreEqual(1, groupedLinks[CPPNNeuronType.Bias]);
            Assert.AreEqual(numberOfInputs, groupedLinks[CPPNNeuronType.Input]);
            Assert.IsFalse(groupedLinks.ContainsKey(CPPNNeuronType.Hidden));
            Assert.IsFalse(groupedLinks.ContainsKey(CPPNNeuronType.Output));
        }

        [TestCase(ExpectedException=typeof(ApplicationException))]
        public void TestEnforceNumberOfInputs()
        {
            var newGA = new CPPNNEATGA(0, 0, null, new List<Func<double, double>>() { null });
        }
    }
}
