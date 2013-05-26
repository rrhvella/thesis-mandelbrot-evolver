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

namespace ComplexCPPNNEAT
{
    public enum CPPNNeuronType
    {
        Bias = 0,
        Input,
        Hidden,
        Output
    }

    public class CPPNNEATNeuronGene
    {
        private int innovationNumber;

        public double Level
        {
            get;
            private set;
        }

        public CPPNNeuronType Type
        {
            get;
            private set;
        }

        public CPPNNEATNeuronGene(int innovationNumber, double level,
CPPNNeuronType type, CPPNNEATActivationFunction activationFunction)
        {
            this.innovationNumber = innovationNumber;
            this.Level = level;

            this.Type = type;
            this.ActivationFunction = activationFunction;
        }

        public CPPNNEATActivationFunction ActivationFunction
        {
            get;
            internal set;
        }

        public void Update()
        {
            switch (Type)
            {
                case CPPNNeuronType.Input:
                    Phene = new CPPNInputNeuron();
                    break;

                case CPPNNeuronType.Output:
                    Phene = new CPPNOutputNeuron(ActivationFunction.Function);
                    break;

                case CPPNNeuronType.Hidden:
                    Phene = new CPPNHiddenNeuron(ActivationFunction.Function);
                    break;

                case CPPNNeuronType.Bias:
                    Phene = new CPPNBiasNeuron();
                    break;
            }
        }

        public CPPNNetworkNeuron Phene
        {
            get;
            private set;
        }

        public string TypeString()
        {
            switch (Type)
            {
                case CPPNNeuronType.Bias:
                    return "B";

                case CPPNNeuronType.Output:
                    return "O";

                case CPPNNeuronType.Input:
                    return "I";

                case CPPNNeuronType.Hidden:
                    return "H";

                default:
                    return "";
            }
        }

        public override string ToString()
        {
            if (Type == CPPNNeuronType.Bias || Type == CPPNNeuronType.Input)
            {
                return String.Format("{0}({1})", TypeString(), innovationNumber);
            }

            return String.Format("{0}({1}, {2})", TypeString(), innovationNumber, ActivationFunction);
        }
    }
}