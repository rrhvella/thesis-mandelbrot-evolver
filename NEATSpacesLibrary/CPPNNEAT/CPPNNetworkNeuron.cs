using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using CPPNNEAT.Extensions;

namespace CPPNNEAT.CPPNNEAT
{
    /// <summary>
    /// Represents a neuron in a CPPN network.
    /// </summary>
    public abstract class CPPNNetworkNeuron
    {
        /// <summary>
        /// The activation of the neuron.
        /// </summary>
        public abstract Complex Activation
        {
            get;
        }
    }

    /// <summary>
    /// CPPN bias neuron.
    /// </summary>
    public class CPPNBiasNeuron: CPPNNetworkNeuron
    {
        public override Complex Activation
        {
            get 
            {
                return 1.0;
            }
        }
    }

    /// <summary>
    /// CPPN input neuron.
    /// </summary>
    public class CPPNInputNeuron: CPPNNetworkNeuron
    {
        private Complex activation;
        public override Complex Activation
        {
            get 
            {
                return activation;
            }
        }

        /// <summary>
        /// Sets the activation of this input neuron.
        /// </summary>
        /// <param name="input"></param>
        public void SetInput(Complex activationValue)
        {
            activation = activationValue;
        }
    }

    /// <summary>
    /// CPPN output neuron.
    /// </summary>
    public class CPPNOutputNeuron: CPPNNetworkNeuron
    {
        /// <summary>
        /// Represents a link from another neuron into this neuron.
        /// </summary>
        private class Synapse
        {
            /// <summary>
            /// The neuron which is feeding into this neuron.
            /// </summary>
            public CPPNNetworkNeuron Neuron
            {
                get;
                private set;
            }

            /// <summary>
            /// The weight of the link.
            /// </summary>
            public Complex Weight
            {
                get;
                private set;
            }

            /// <summary>
            /// </summary>
            /// <param name="neuron">The neuron which is feeding into this neuron.</param>
            /// <param name="weight">The weight of the connection.</param>
            public Synapse(CPPNNetworkNeuron neuron, Complex weight)
            {
                this.Neuron = neuron;
                this.Weight = weight;
            }
        }

        /// <summary>
        /// The links going into this neuron.
        /// </summary>
        private List<Synapse> synapsis;

        /// <summary>
        /// The activation function of this neuron.
        /// </summary>
        private Func<Complex, Complex> activationFunction;

        /// <summary>
        /// Is true if this neuron is currently calculating its activation.
        /// </summary>
        private bool isCalculating;

        /// <summary>
        /// The activation of this neuron the last time it was excited.
        /// </summary>
        private Complex previousActivation;

        public override Complex Activation
        {
            get 
            {
                //If this neuron is already calculating its activation, break 
                //the cycle and returns its activation the last time it was 
                //excited.
                if (isCalculating)
                {
                    return previousActivation;
                }

                isCalculating = true;

                Complex net = 0;
                
                //Recursively retrieve the activations of the neurons feeding into this
                //neuron.
                foreach(var synapse in synapsis) 
                {
                    net += synapse.Neuron.Activation * synapse.Weight;
                }

                var activation = activationFunction(net);

                isCalculating = false;

                previousActivation = activation;

                return activation;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="activationFunction">The activation function of the output neuron.</param>
        public CPPNOutputNeuron(Func<Complex, Complex> activationFunction)
        {
            this.activationFunction = activationFunction;
            this.synapsis = new List<Synapse>();
            this.isCalculating = false;
        }

        /// <summary>
        /// Adds a new connection from the given neuron to this neuron, with the given weight.
        /// </summary>
        /// <param name="neuron"></param>
        /// <param name="weight"></param>
        public void AddChild(CPPNNetworkNeuron neuron, Complex weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }

        /// <summary>
        /// Resets the activation of this neuron to 0.
        /// </summary>
        /// <remarks>
        /// This would be used if the neuron were recurrent.
        /// </remarks>
        public void Reset()
        {
            previousActivation = 0;
        }
    }

    /// <summary>
    /// CPPN hidden neuron.
    /// </summary>
    public class CPPNHiddenNeuron: CPPNOutputNeuron
    {
        /// <summary>
        /// </summary>
        /// <param name="activationFunction">The activation function of the hidden neuron.</param>
        public CPPNHiddenNeuron(Func<Complex, Complex> activationFunction): base(activationFunction)
        {
        }
    }
}
