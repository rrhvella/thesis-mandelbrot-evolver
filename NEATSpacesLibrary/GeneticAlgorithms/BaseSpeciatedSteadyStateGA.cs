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

        public BaseSpeciatedSteadyStateGA(int populationSize, Func<GenomeType, double> scoreFunction)
            : base(populationSize, scoreFunction)
        {
            GenomeAdded += new EventHandler<GenomeEventArgs<GenomeType>>(SpeciatedSteadyStateGA_GenomeAddedEventHandler);
            GenomeRemoved += new EventHandler<GenomeEventArgs<GenomeType>>(SpeciatedSteadyStateGA_GenomeRemovedEventHandler);
            IterationComplete += new EventHandler<EventArgs>(SpeciatedSteadyStateGA_IterationComplete);

            populationSpecies = new List<Species<GType, PType>>();
        }

        private void SpeciatedSteadyStateGA_IterationComplete(object sender, EventArgs e)
        {
            foreach (var species in populationSpecies.Where(elem => elem.CanBreed).ToList())
            {
                if (++(species.TotalIterations) >= NoInnovationThreshold)
                {
                    species.CanBreed = false;
                }
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
            var tournament = Population.Where(elem => elem.Species.CanBreed)
                                .ToList().RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                .OrderByDescending(elem => elem.Score)
                                .ToArray();

            var partner = (GenomeType)tournament[1];

            if ((new Random()).NextDouble() > InterSpeciesMatingRate)
            {
                partner = (GenomeType)tournament[0].Species.Members
                                    .ToList().RandomTake(DEFAULT_TOURNAMENT_SIZE)
                                    .OrderByDescending(elem => elem.Score)
                                    .First();
            }

            return new GASelectionResult<GenomeType>(tournament[0], (GenomeType)partner, tournament[DEFAULT_TOURNAMENT_SIZE - 1],
                                                    tournament[DEFAULT_TOURNAMENT_SIZE - 2]);

        }
    }
}
