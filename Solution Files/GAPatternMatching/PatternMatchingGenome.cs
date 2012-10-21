using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace GAPatternMatching
{
    public class PatternMatchingGenome: CollectionGenome<PatternMatchingGenome, int[], int>
    {
        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        public PatternMatchingGenome() : base(MUTATION_RATE, CROSSOVER_RATE)
        {
            GeneCollection = new int[PatternMatchingConstants.TARGET_PATTERN.Length];
        }

        public override void Initialise()
        {
            for (int i = 0; i < GeneCollection.Length; i++)
            {
                GeneCollection[i] = (Parent.Random.NextDouble() > 0.5) ? 1 : 0;
            }
        }

        protected override int GetMutatedElement(int currentValue)
        {
            return Math.Abs(currentValue - 1);
        }
    }
}
