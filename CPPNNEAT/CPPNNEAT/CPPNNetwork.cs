using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace CPPNNEAT.CPPNNEAT
{
    /// <summary>
    /// Represents a CPPN network.
    /// </summary>
    public class CPPNNetwork
    {
        /// <summary>
        /// The neurons in this network.
        /// </summary>
        public HashSet<CPPNNetworkNeuron> Neurons 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// The hidden neurons in this network.
        /// </summary>
        public IEnumerable<CPPNHiddenNeuron> HiddenNeurons
        {
            get
            {
                return hiddenNeurons.AsReadOnly();
            }
        }

        /// <summary>
        /// The hidden neurons in this network.
        /// </summary>
        public IEnumerable<CPPNInputNeuron> InputNeurons
        {
            get
            {
                return inputNeurons.AsReadOnly();
            }
        }

        private List<CPPNInputNeuron> inputNeurons;

        /// <summary>
        /// The hidden neurons in this network.
        /// </summary>
        private List<CPPNHiddenNeuron> hiddenNeurons;

        /// <summary>
        /// The output neurons in this network.
        /// </summary>
        private CPPNOutputNeuron outputNeuron;

        public CPPNNetwork()
        {
            this.Neurons = new HashSet<CPPNNetworkNeuron>();
            this.hiddenNeurons = new List<CPPNHiddenNeuron>();
            this.inputNeurons = new List<CPPNInputNeuron>();
        }

        /// <summary>
        /// Returns the output neuron's activation given the specified network input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a link with the given weight between the specified neurons.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        public void AddLink(CPPNNetworkNeuron from, CPPNNetworkNeuron to, Complex weight)
        {
            if (Neurons.Contains(from) && Neurons.Contains(to))
            {
                (to as CPPNOutputNeuron).AddChild(from, weight);
            }
        }
        
        /// <summary>
        /// Adds the given neuron to the network.
        /// </summary>
        /// <param name="neuron"></param>
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

        /// <summary>
        /// The number of neurons in the network.
        /// </summary>
        public int NeuronCount
        {
            get
            {
                return Neurons.Count;
            }
        }

        /// <summary>
        /// Resets the activation of the recurrent neurons to 0.
        /// </summary>
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


