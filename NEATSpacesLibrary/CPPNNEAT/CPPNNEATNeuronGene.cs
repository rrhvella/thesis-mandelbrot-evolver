using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public enum CPPNNeuronType
    {
        Bias = 0,
        Input,
        Hidden,
        Output
    }

    public class CPPNNEATNeuronGene: IDebugabble
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
                                CPPNNeuronType type, Func<Complex, Complex> activationFunction)
        {
            this.innovationNumber = innovationNumber;
            this.Level = level;

            this.Type = type;
            this.ActivationFunction = activationFunction;
        }

        public Func<Complex, Complex> ActivationFunction
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
                    Phene = new CPPNOutputNeuron(ActivationFunction);
                    break;

                case CPPNNeuronType.Hidden:
                    Phene = new CPPNHiddenNeuron(ActivationFunction);
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

        public string DebugInformation()
        {
            var result = new StringBuilder();

            switch (Type)
            {
                case CPPNNeuronType.Bias:
                    result.Append("B");
                    break;

                case CPPNNeuronType.Output:
                    result.Append("O");
                    break;

                case CPPNNeuronType.Input:
                    result.Append("I");
                    break;

                case CPPNNeuronType.Hidden:
                    result.Append("H");
                    break;
            }

            result.AppendFormat("({0})", innovationNumber);

            return result.ToString();
        }
    }
}
