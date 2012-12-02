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


        public string DebugInformation()
        {
            return String.Format("{0}({1}, {2})", TypeString(), innovationNumber, ActivationFunction);
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", TypeString(), ActivationFunction);
        }
    }
}
