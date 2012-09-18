using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;

namespace NEATSpacesTests.CPPNNEAT
{
    public class CPPNNEATConstants
    {
        public const int NUMBER_OF_INPUTS = 2;
        public const double BIAS = -0.5;
        public const double WEIGHT_1 = 0.5;
        public const double WEIGHT_2 = -0.5;
        public const double WEIGHT_3 = 2.0;
        public const double WEIGHT_4 = 1.5;
        public const double WEIGHT_5 = 0.5;
        public static readonly Func<double, double> OUTPUT_ACTIVATION_FUNCTION = CPPNActivationFunctions.TanHActivationFunction;
        public static readonly Func<double, double> HIDDEN_ACTIVATION_FUNCTION = CPPNActivationFunctions.TanHActivationFunction;

        private static double Level1(double input1, double input2) 
        {
            return BIAS + WEIGHT_1 * input1 + WEIGHT_2 * input2;
        }

        private static double Level2(double input1, double input2) 
        {
            return HIDDEN_ACTIVATION_FUNCTION(Level1(input1, input2)) * WEIGHT_3;
        }

        public static double DefaultActivation(double input1, double input2)
        {
            return OUTPUT_ACTIVATION_FUNCTION(Level1(input1, input2));
        }

        public static double RecursiveActivation1(double input1, double input2) 
        {
            return OUTPUT_ACTIVATION_FUNCTION(Level2(input1, input2));
        }

        public static double RecursiveActivation2(double input1, double input2, double output) 
        {
            return OUTPUT_ACTIVATION_FUNCTION(HIDDEN_ACTIVATION_FUNCTION(Level1(input1, input2) + output * WEIGHT_4) * WEIGHT_3);
        }

        public static double ParallelActivation(double input1, double input2) 
       {
            return OUTPUT_ACTIVATION_FUNCTION(Level2(input1, input2) + 
                    HIDDEN_ACTIVATION_FUNCTION(Level1(input1, input2)) * WEIGHT_4);
        }
    }
}
