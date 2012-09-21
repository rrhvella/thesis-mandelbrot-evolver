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

        protected override GASelectionResult<GenomeType, GType, PType> PerformSelection()
        {
            var individualsToReplace = Population.OrderBy(member => member.AdjustedScore).Take(2).ToArray();
            var totalFitness = populationSpecies.Select(species => species.AverageFitness).Sum();

            var parentSpecies = populationSpecies
                                        .Where(species => species.CanBreed)
                                        .RouletteWheelTake(species => species.AverageFitness / totalFitness, 2)
                                        .ToArray();

            return new GASelectionResult<GenomeType, GType, PType>((GenomeType)parentSpecies[0].Best, 
                                                                (GenomeType)parentSpecies[1].Best,
                                                                individualsToReplace[0], individualsToReplace[1]);
        }

        protected override string InnerDebugInformation()
        {
            var result = new StringBuilder();

            foreach (var i in Enumerable.Range(0, populationSpecies.Count))
            {
                result.Append("Species ");
                result.AppendLine(i.ToString());

                result.AppendLine();

                result.AppendLine(populationSpecies[i].DebugInformation());
            }

            return result.ToString();
        }
    }
}
