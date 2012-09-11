using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNetwork
    {
        private HashSet<CPPNNetworkNeuron> neurons;

        private List<CPPNInputNeuron> inputNeurons;
        private CPPNOutputNeuron outputNeuron;

        public CPPNNetwork()
        {
            this.neurons = new HashSet<CPPNNetworkNeuron>();
            this.inputNeurons = new List<CPPNInputNeuron>();
        }

        public double GetActivation(double[] input)
        {
            if (input.Length != inputNeurons.Count)
            {
                throw new ApplicationException(String.Format("There are {0} input neurons in this network," + 
                                                        " please specify an array with {0} elements",
                                                        inputNeurons.Count));
            }

            foreach (var i in Enumerable.Range(0, input.Length))
            {
                inputNeurons[i].SetInput(input[i]);
            }

            return outputNeuron.Activation;
        }

        public void AddLink(CPPNNetworkNeuron from, CPPNNetworkNeuron to, double weight)
        {
            if (neurons.Contains(from) && neurons.Contains(to))
            {
                (to as CPPNOutputNeuron).AddChild(from, weight);
            }
        }

        public void AddNeuron(CPPNNetworkNeuron neuron)
        {
            neurons.Add(neuron);

            if (neuron is CPPNInputNeuron)
            {
                inputNeurons.Add((CPPNInputNeuron)neuron);
            }

            if (neuron.GetType() == typeof(CPPNOutputNeuron))
            {
                outputNeuron = (CPPNOutputNeuron)neuron;
            }
        }
    }
}
