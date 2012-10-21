using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.CPPNNEAT;

namespace GAPatternMatching
{
    public class Program: PatternMatchingForm<SteadyStateGA<PatternMatchingGenome, int[], int[]>, 
                                               PatternMatchingGenome, int[], int[]>
    {
        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override int[] TransformPhenome(int[] phenome)
        {
            return phenome;
        }

        protected override SteadyStateGA<PatternMatchingGenome, int[], int[]> CreateGAListGA(int populationSize, Func<PatternMatchingGenome, double> scoreFunction)
        {
            var result = new SteadyStateGA<PatternMatchingGenome, int[], int[]>(populationSize, scoreFunction);
            return result;
        }
    }
}

