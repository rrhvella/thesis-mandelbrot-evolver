using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ComplexCPPNNEAT
{
    public class CPPNNetwork
    {
        public HashSet<CPPNNetworkNeuron> Neurons
        {
            get;
            private set;
        }

        public IEnumerable<CPPNHiddenNeuron> HiddenNeurons
        {
            get
            {
                return hiddenNeurons.AsReadOnly();
            }
        }

        public IEnumerable<CPPNInputNeuron> InputNeurons
        {
            get
            {
                return inputNeurons.AsReadOnly();
            }
        }

        private List<CPPNInputNeuron> inputNeurons;

        private List<CPPNHiddenNeuron> hiddenNeurons;

        private CPPNOutputNeuron outputNeuron;

        public CPPNNetwork()
        {
            this.Neurons = new HashSet<CPPNNetworkNeuron>();
            this.hiddenNeurons = new List<CPPNHiddenNeuron>();
            this.inputNeurons = new List<CPPNInputNeuron>();
        }

        public Complex GetActivation(Complex[] input)
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

        public void AddLink(CPPNNetworkNeuron from, CPPNNetworkNeuron to, Complex weight)
        {
            if (Neurons.Contains(from) && Neurons.Contains(to))
            {
                (to as CPPNOutputNeuron).AddChild(from, weight);
            }
        }

        public void AddNeuron(CPPNNetworkNeuron neuron)
        {
            Neurons.Add(neuron);

            if (neuron is CPPNInputNeuron)
            {
                inputNeurons.Add((CPPNInputNeuron)neuron);
            }
            else if (neuron.GetType() == typeof(CPPNOutputNeuron))
            {
                outputNeuron = (CPPNOutputNeuron)neuron;
            }
            else if (neuron.GetType() == typeof(CPPNHiddenNeuron))
            {
                hiddenNeurons.Add((CPPNHiddenNeuron)neuron);
            }
        }

        public int NeuronCount
        {
            get
            {
                return Neurons.Count;
            }
        }

        public void Reset()
        {
            foreach (var hiddenNeuron in hiddenNeurons)
            {
                hiddenNeuron.Reset();
            }

            outputNeuron.Reset();
        }
    }
}