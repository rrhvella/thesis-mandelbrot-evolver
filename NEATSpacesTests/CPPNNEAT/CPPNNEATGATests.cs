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

        [TestCase]
        public void TestInnovationNumberOfDefaultGenes()
        {
            var newGA = new CPPNNEATGA(2, 0, null, new List<Func<double, double>>() { null });

            foreach(var i in Enumerable.Range(0, 3)) {
                Assert.AreEqual(newGA.DefaultLinkGenes[i].InnovationNumber,
                                newGA.GetInnovationNumber(newGA.DefaultNeuronGenes[i], newGA.DefaultNeuronGenes[3]));
            }
        }

        [TestCase]
        public void TestInnovationNumberOfNewGenes()
        {
            var newGA = new CPPNNEATGA(2, 0, null, new List<Func<double, double>>() { null });

            var from = new CPPNNEATNeuronGene(CPPNNeuronType.Input, null);
            var to = new CPPNNEATNeuronGene(CPPNNeuronType.Input, null);

            Assert.AreEqual(3, newGA.GetInnovationNumber(from, to));
            Assert.AreEqual(3, newGA.GetInnovationNumber(from, to));
        }

        [TestCase]
        public void TestInnovationNumberOfCreateNeuron()
        {
            var newGA = new CPPNNEATGA(2, 1, x => 0.0, new List<Func<double, double>>() { null });
            newGA.Initialise();

            var targetGeneCollection = newGA.Population[0].GeneCollection;
                
            targetGeneCollection.CreateNeuronGene(0);

            Assert.AreEqual(targetGeneCollection.LinkGenes[3].InnovationNumber,
                    newGA.GetInnovationNumber(targetGeneCollection.LinkGenes[3].From, targetGeneCollection.LinkGenes[3].To));

            Assert.AreEqual(4,
                    newGA.GetInnovationNumber(targetGeneCollection.LinkGenes[4].From, targetGeneCollection.LinkGenes[4].To));
        }

        [TestCase]
        public void TestInnovationNumberOfCreateLink()
        {
            var newGA = new CPPNNEATGA(2, 1, x => 0.0, new List<Func<double, double>>() { null });
            newGA.Initialise();

            var targetGeneCollection = newGA.Population[0].GeneCollection;
                
            targetGeneCollection.CreateNeuronGene(1);
            targetGeneCollection.TryCreateLinkGene(1, 4);

            Assert.AreEqual(targetGeneCollection.LinkGenes[5].InnovationNumber,
                    newGA.GetInnovationNumber(targetGeneCollection.LinkGenes[5].From, targetGeneCollection.LinkGenes[5].To));
        }
    }
}
