using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class SteadyStateGA<GenomeType, GType, PType> : 
            BaseSteadyStateGA<GenomeType, GType, PType> where GenomeType : Genome<GType, PType>, new()
    {
        public SteadyStateGA(int populationSize): base(populationSize)
        {
        }

        protected override GASelectionResult<GenomeType> PerformSelection()
        {
            var tournament = Population.RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                        .OrderByDescending(elem => elem.Score)
                                        .ToArray();

            return new GASelectionResult<GenomeType>(tournament[0], tournament[1], 
                    tournament[DEFAULT_TOURNAMENT_SIZE - 2], tournament[DEFAULT_TOURNAMENT_SIZE - 1]);
        }
    }
}
