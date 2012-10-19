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
    public abstract class MapForm<GAType, GenomeType, GType, PType>: 
            GAExperiment<GAType, GenomeType, GType, PType>
            where GAType : BaseGA<GenomeType, GType, PType>
            where GenomeType : Genome<GType, PType>, new()
    {
        private const int NUMBER_OF_GENERATIONS = 10;
        private const int NUMBER_OF_RUNS = 100;

        private const int MATING_EVENTS_PER_GENERATION = 20;
        private const int TOTAL_TICKS = MATING_EVENTS_PER_GENERATION * NUMBER_OF_GENERATIONS;

        private const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 10;

        private const int POPULATION_SIZE = 10;

        public MapForm(): base(NUMBER_OF_GENERATIONS, NUMBER_OF_RUNS, MATING_EVENTS_PER_GENERATION, NUMBER_OF_TICKS_TO_UPDATE_IMAGE,
                            POPULATION_SIZE)
        {
        }

        protected abstract Map TransformPhenome(PType phenome);

        protected override Image GenerateImage(PType phenome)
        {
            return TransformPhenome(phenome).Image;
        }

        protected override double ScoreFunction(GenomeType genome)
        {
            return TransformPhenome(genome.Phenome).DistanceFromStartToEnd;
        }
    }
}
