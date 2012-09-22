using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public abstract class BaseSpeciatedGA<GenomeType, GType, PType> : 
            BaseGA<GenomeType, GType, PType>,
            ISpeciatedGA, 
            IDebugabble
            where GenomeType : SpeciatedGenome<GType, PType>, new()
    {
        public double InterSpeciesMatingRate { get; set; }
        public double CompatibilityDistanceThreshold { get; set; }
        public int NoInnovationThreshold { get; set; }
        private List<Species<GType, PType>> populationSpecies;
        private int SPECIES_CHAMPION_THRESHOLD = 5;

        public bool Failed
        {
            get
            {
                return !populationSpecies.Any(species => species.CanBreed);
            }
        }

        public BaseSpeciatedGA(int populationSize, Func<GenomeType, double> scoreFunction)
            : base(populationSize, scoreFunction)
        {
            GenomeAdded += new EventHandler<GenomeEventArgs<GenomeType>>(BaseSpeciatedGA_GenomeAddedEventHandler);
            GenomeRemoved += new EventHandler<GenomeEventArgs<GenomeType>>(BaseSpeciatedGA_GenomeRemovedEventHandler);
            SelectionComplete += new EventHandler<EventArgs>(BaseSpeciatedGA_SelectionComplete);
            IterationComplete += new EventHandler<EventArgs>(BaseSpeciatedGA_IterationComplete);

            populationSpecies = new List<Species<GType, PType>>();
        }

        public void BaseSpeciatedGA_SelectionComplete(object sender, EventArgs e)
        {
            foreach (var species in populationSpecies)
            {
                species.Clear();
            }
        }

        public new void SteadyStateIterate()
        {
            if (Failed)
            {
                return;
            }

            base.SteadyStateIterate();
        }

        private void BaseSpeciatedGA_IterationComplete(object sender, EventArgs e)
        {
            foreach (var species in populationSpecies.ToArray())
            {
                if (species.CanBreed && species.Count > 0)
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
                else if (species.Count == 0)
                {
                    populationSpecies.Remove(species);
                }
            }
        }

        private void BaseSpeciatedGA_GenomeRemovedEventHandler(object sender, GenomeEventArgs<GenomeType> e)
        {
            e.Genome.Species.Remove(e.Genome);
        }

        private void BaseSpeciatedGA_GenomeAddedEventHandler(object sender, GenomeEventArgs<GenomeType> e)
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

        protected override GASteadyStateSelectionResult<GenomeType, GType, PType> PerformSteadyStateSelection()
        {
            var individualsToReplace = Population.OrderBy(member => member.AdjustedScore).Take(2).ToArray();
            var totalFitness = populationSpecies.Select(species => species.AverageFitness).Sum();

            var parents = populationSpecies
                                        .Where(species => species.CanBreed)
                                        .RouletteWheelSingle(species => species.AverageFitness / totalFitness)
                                        .Members
                                        .Cast<GenomeType>()
                                        .ToArray();
            var parent = parents[0];
            var partner = parent;

            if (parents.Length > 1)
            {
                partner = parents[1];
            }


            return new GASteadyStateSelectionResult<GenomeType, GType, PType>(parent, partner,
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

        protected override GAGenerationalSelectionResult<GenomeType, GType, PType> PerformGenerationalSelection()
        {
            var result = new GAGenerationalSelectionResult<GenomeType, GType, PType>();
            var currentPopulationSize = Population.Count;

            Action<IEnumerable<GenomeType>, int> AddToResult = 
                delegate(IEnumerable<GenomeType> candidates, int amount) {
                    var genomeList = candidates.Take((int)Math.Ceiling(candidates.Count() * 0.3))
                                        .ToList();

                    var crossoverAmount = (int)Math.Floor(amount * CrossoverRate);
                    var mutationAmount = (int)Math.Ceiling(amount * (1 - CrossoverRate));

                    foreach (var i in Enumerable.Range(0, crossoverAmount))
                    {
                        if(result.Count == currentPopulationSize) 
                        {
                            break;
                        }

                        result.ParentPairs.Add(Tuple.Create(genomeList.RandomSingle(),
                                                            genomeList.RandomSingle()
                                                            ));
                    }

                    foreach (var i in Enumerable.Range(0, mutationAmount))
                    {
                        if(result.Count == currentPopulationSize) 
                        {
                            break;
                        }

                        result.ToMutate.Add((GenomeType)genomeList.RandomSingle().Copy());
                    }
                };
            
            //Add champions.
            var totalAverageFitness = 0.0;

            foreach (var species in populationSpecies)
            {
                if (species.Count > SPECIES_CHAMPION_THRESHOLD)
                {
                    result.ToRetain.Add((GenomeType)species.Best.Copy());
                }

                totalAverageFitness += species.AverageFitness;
            }

            var populationSizeAfterChampions = currentPopulationSize - result.ToRetain.Count;

            //Add parent pairs and to mutate.
            foreach (var species in populationSpecies.Where(species => species.CanBreed))
            {
                if(result.Count == currentPopulationSize) 
                {
                    break;
                }

                var breedingLimit = (int)Math.Ceiling(species.AverageFitness / 
                                                        totalAverageFitness * populationSizeAfterChampions);

                AddToResult(species.Members.Cast<GenomeType>(), breedingLimit);
            }

            if (currentPopulationSize > result.Count)
            {
                var amount = currentPopulationSize - result.Count;

                AddToResult(Population, amount);
            }

            return result;
        }

        public double CrossoverRate { get; set; }
    }
}
