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