using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class SteadyStateGA<GenomeType, GType, PType> : 
            BaseGA<GenomeType, GType, PType> 
            where GenomeType : Genome<GType, PType>, new()
    {
        public SteadyStateGA(int populationSize, Func<GenomeType, double> scoreFunction): base(populationSize, scoreFunction)
        {
        }

        protected override GASteadyStateSelectionResult<GenomeType, GType, PType> PerformSteadyStateSelection()
        {
            var tournament = Population.RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                        .OrderByDescending(elem => elem.AdjustedScore)
                                        .ToArray();

            return new GASteadyStateSelectionResult<GenomeType, GType, PType>(tournament[0], tournament[1], 
                        tournament[DEFAULT_TOURNAMENT_SIZE - 2], 
                        tournament[DEFAULT_TOURNAMENT_SIZE - 1]);
        }

        protected override GAGenerationalSelectionResult<GenomeType, GType, PType> PerformGenerationalSelection()
        {
            throw new NotImplementedException();
        }
    }
}
