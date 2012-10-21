using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace NEATSpacesLibrary.NEATSpaces
{
    public abstract class PatternMatchingForm<GAType, GenomeType, GType, PType>: 
            GAExperiment<GAType, GenomeType, GType, PType>
            where GAType : BaseGA<GenomeType, GType, PType>
            where GenomeType : Genome<GType, PType>, new()
    {
        private const int NUMBER_OF_GENERATIONS = 50;
        private const int NUMBER_OF_RUNS = 100;

        private const int MATING_EVENTS_PER_GENERATION = 100;
        private const int TOTAL_TICKS = MATING_EVENTS_PER_GENERATION * NUMBER_OF_GENERATIONS;

        private const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 10;

        private const int POPULATION_SIZE = 120;

        public PatternMatchingForm(): base(NUMBER_OF_GENERATIONS, NUMBER_OF_RUNS, MATING_EVENTS_PER_GENERATION, 
                            NUMBER_OF_TICKS_TO_UPDATE_IMAGE, POPULATION_SIZE)
        {
        }

        protected abstract int[] TransformPhenome(PType phenome);

        protected override Image GenerateImage(PType phenome)
        {
            return null;
        }

        protected override double ScoreFunction(GenomeType genome)
        {
            var generatedPattern = TransformPhenome(genome.Phenome);
            return generatedPattern.Zip(PatternMatchingConstants.TARGET_PATTERN, 
                                    (first, second) => (first == second) ? 1 : 0).Sum();
        }
    }
}
