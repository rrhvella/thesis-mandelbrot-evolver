using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class SteadyStateGA<GenomeType, GType, PType> : 
            BaseSteadyStateGA<GenomeType, GType, PType> 
            where GenomeType : Genome<GType, PType>, new()
    {
        public SteadyStateGA(int populationSize, Func<GenomeType, double> scoreFunction): base(populationSize, scoreFunction)
        {
        }

        protected override GASelectionResult<GenomeType, GType, PType> PerformSelection()
        {
            var tournament = Population.RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                        .OrderByDescending(elem => elem.AdjustedScore)
                                        .ToArray();

            return new GASelectionResult<GenomeType, GType, PType>(tournament[0], tournament[1], 
                        tournament[DEFAULT_TOURNAMENT_SIZE - 2], 
                        tournament[DEFAULT_TOURNAMENT_SIZE - 1]);
        }
    }
}
