/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace ComplexCPPNNEAT
{
    public abstract class CPPNNetworkNeuron
    {
        public abstract Complex Activation
        {
            get;
        }
    }

    public class CPPNBiasNeuron : CPPNNetworkNeuron
    {
        public override Complex Activation
        {
            get
            {
                return 1.0;
            }
        }
    }

    public class CPPNInputNeuron : CPPNNetworkNeuron
    {
        private Complex activation;

        public override Complex Activation
        {
            get
            {
                return activation;
            }
        }

        public void SetInput(Complex activationValue)
        {
            activation = activationValue;
        }
    }

    public class CPPNOutputNeuron : CPPNNetworkNeuron
    {
        private class Synapse
        {
            public CPPNNetworkNeuron Neuron
            {
                get;
                private set;
            }

            public Complex Weight
            {
                get;
                private set;
            }

            public Synapse(CPPNNetworkNeuron neuron, Complex weight)
            {
                this.Neuron = neuron;
                this.Weight = weight;
            }
        }

        private List<Synapse> synapsis;

        private Func<Complex, Complex> activationFunction;

        private bool isCalculating;

        private Complex previousActivation;

        public override Complex Activation
        {
            get
            {
                if (isCalculating)
                {
                    return previousActivation;
                }

                isCalculating = true;

                Complex net = 0;

                foreach (var synapse in synapsis)
                {
                    net += synapse.Neuron.Activation * synapse.Weight;
                }

                var activation = activationFunction(net);

                isCalculating = false;

                previousActivation = activation;

                return activation;
            }
        }

        public CPPNOutputNeuron(Func<Complex, Complex> activationFunction)
        {
            this.activationFunction = activationFunction;
            this.synapsis = new List<Synapse>();
            this.isCalculating = false;
        }

        public void AddChild(CPPNNetworkNeuron neuron, Complex weight)
        {
            synapsis.Add(new Synapse(neuron, weight));
        }

        public void Reset()
        {
            previousActivation = 0;
        }
    }

    public class CPPNHiddenNeuron : CPPNOutputNeuron
    {
        public CPPNHiddenNeuron(Func<Complex, Complex> activationFunction)
            : base(activationFunction)
        {
        }
    }
}