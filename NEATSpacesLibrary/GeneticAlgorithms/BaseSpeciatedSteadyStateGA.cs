using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class BaseSpeciatedSteadyStateGA<GenomeType, GType, PType> : 
            BaseSteadyStateGA<GenomeType, GType, PType>,
            ISpeciatedGA
            where GenomeType : SpeciatedGenome<GType, PType>, new()
    {
        public double InterSpeciesMatingRate { get; set; }
        public double CompatibilityDistanceThreshold { get; set; }
        public int NoInnovationThreshold { get; set; }
        private List<Species<GType, PType>> populationSpecies;

        public bool Failed
        {
            get
            {
                return !populationSpecies.Any(species => species.CanBreed);
            }
        }

        public BaseSpeciatedSteadyStateGA(int populationSize, Func<GenomeType, double> scoreFunction)
            : base(populationSize, scoreFunction)
        {
            GenomeAdded += new EventHandler<GenomeEventArgs<GenomeType>>(SpeciatedSteadyStateGA_GenomeAddedEventHandler);
            GenomeRemoved += new EventHandler<GenomeEventArgs<GenomeType>>(SpeciatedSteadyStateGA_GenomeRemovedEventHandler);
            IterationComplete += new EventHandler<EventArgs>(SpeciatedSteadyStateGA_IterationComplete);

            populationSpecies = new List<Species<GType, PType>>();
        }


        public new void Iterate()
        {
            if (Failed)
            {
                return;
            }

            base.Iterate();
        }

        private void SpeciatedSteadyStateGA_IterationComplete(object sender, EventArgs e)
        {
            var breedingSpecies = populationSpecies.Where(elem => elem.CanBreed);

            foreach (var species in breedingSpecies)
            {
                if (species.Best.Score <= species.PreviousScore)
                {
                    if (++(species.TotalIterationsWithNoInnovation) >= NoInnovationThreshold)
                    {
                        species.CanBreed = false;
                    }
                }
                else
                {
                    species.TotalIterationsWithNoInnovation = 0;
                }

                species.PreviousScore = species.Best.Score;
            }
        }

        private void SpeciatedSteadyStateGA_GenomeRemovedEventHandler(object sender, GenomeEventArgs<GenomeType> e)
        {
            e.Genome.Species.Remove(e.Genome);

            if (e.Genome.Species.Count == 0)
            {
                populationSpecies.Remove(e.Genome.Species);
            }
        }

        private void SpeciatedSteadyStateGA_GenomeAddedEventHandler(object sender, GenomeEventArgs<GenomeType> e)
        {
            foreach(var species in populationSpecies) 
            {
                if (species.BelongsTo(e.Genome))
                {
                    species.Add(e.Genome);
                    e.Genome.Species = species;

                    return;
                }
            }

            var newSpecies = new Species<GType, PType>(this, e.Genome);
            populationSpecies.Add(newSpecies);

            e.Genome.Species = newSpecies;
        }

        protected override GASelectionResult<GenomeType> PerformSelection()
        {
            var tournamentSuccessful = Population.Where(elem => elem.Species.CanBreed)
                                .ToList().RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                .OrderByDescending(elem => elem.AdjustedScore)
                                .ToArray();

            var tournamentStraglers = Population.ToList().RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                .OrderByDescending(elem => elem.AdjustedScore)
                                .ToArray();

            var partner = (GenomeType)tournamentSuccessful[1];

            if (random.NextDouble() > InterSpeciesMatingRate)
            {
                partner = (GenomeType)tournamentSuccessful[0].Species.Members
                                    .ToList().RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                    .OrderByDescending(elem => elem.AdjustedScore)
                                    .First();
            }

            return new GASelectionResult<GenomeType>(tournamentSuccessful[0], (GenomeType)partner, 
                                                    tournamentStraglers[tournamentStraglers.Length - 1],
                                                    tournamentStraglers[tournamentStraglers.Length - 2]);

        }
    }
}
