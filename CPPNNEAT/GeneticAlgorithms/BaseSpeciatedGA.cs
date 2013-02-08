using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPPNNEAT.Extensions;

namespace CPPNNEAT.GeneticAlgorithms
{
    /// <summary>
    /// Base class for genetic algorithms which use speciation.
    /// </summary>
    /// <typeparam name="GenomeType">The genome type. </typeparam>
    /// <typeparam name="GType">The type of the genome genetic sequence. </typeparam>
    /// <typeparam name="PType">The type of the phenome. </typeparam>
    public abstract class BaseSpeciatedGA<GenomeType, GType, PType> : 
            BaseGA<GenomeType, GType, PType>,
            ISpeciatedGA
            where GenomeType : SpeciatedGenome<GType, PType>, new()
    {
        /// <summary>
        /// The rate at which individuals mate with individuals outside of their species.
        /// </summary>
        public double InterSpeciesMatingRate { get; set; }

        /// <summary>
        /// Determines the point at which an individual is not part of a species, based on its 
        /// distance to the species representative
        /// </summary>
        public double CompatibilityDistanceThreshold { get; set; }

        /// <summary>
        /// Determines the point at which a species is considered stagnant, based on the number
        /// of iterations without an improvement in the best fitness.
        /// </summary>
        public int NoInnovationThreshold { get; set; }

        /// <summary>
        /// The proportion of a species population which is used for breeding.
        /// </summary>
        public double SurvivalTreshold { get; set; }

        /// <summary>
        /// The specie for the genomes in the population.
        /// </summary>
        private List<Species<GType, PType>> populationSpecies;

        /// <summary>
        /// The number of individuals that a species must have in order for its best genome to be 
        /// copied into the next generation without modification.
        /// </summary>
        private int SPECIES_CHAMPION_THRESHOLD = 5;

        public override bool Failed
        {
            get
            {
                return !populationSpecies.Any(species => species.CanBreed && 
                        species.AverageFitness > 0);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="populationSize">The number of individuals in the population.</param>
        /// <param name="scoreFunction">The function used to determine the fitness of an individual.
        /// </param>
        public BaseSpeciatedGA(int populationSize, Func<GenomeType, double> scoreFunction)
            : base(populationSize, scoreFunction)
        {
            GenomeAdded += new EventHandler<GenomeEventArgs<GenomeType>>(
                                                        BaseSpeciatedGA_GenomeAddedEventHandler);
            SelectionComplete += new EventHandler<EventArgs>(BaseSpeciatedGA_SelectionComplete);
            IterationComplete += new EventHandler<IterationEventArgs>(
                                                        BaseSpeciatedGA_IterationComplete);

            populationSpecies = new List<Species<GType, PType>>();
        }

        /// <summary>
        /// Handles the event fired when a selection has been completed.
        /// </summary>
        public void BaseSpeciatedGA_SelectionComplete(object sender, EventArgs e)
        {
            foreach (var species in populationSpecies)
            {
                species.Clear();
            }
        }

        /// <summary>
        /// Handles the event fired when an iteration has been completed.
        /// </summary>
        private void BaseSpeciatedGA_IterationComplete(object sender, IterationEventArgs e)
        {
            //Mark the species which have not improved so they don't breed and remove any species 
            //which don't have any individuals.
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

        /// <summary>
        /// Handles the event fired when a genome has been added to the population.
        /// </summary>
        private void BaseSpeciatedGA_GenomeAddedEventHandler(object sender, 
                                                            GenomeEventArgs<GenomeType> e)
        {
            //Add this genome to a species it is compatible. If it's not compatible to any species,
            //create a new species with this genome as its representative.
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

        /// <summary>
        /// Yields a number of individuals from the given enumerable based on the survival treshold.
        /// </summary>
        /// <param name="candidates"></param>
        /// <returns></returns>
        private IList<GenomeType> SelectBreeders(IEnumerable<GenomeType> candidates)
        {
            return candidates.Take((int)Math.Ceiling(candidates.Count() * SurvivalTreshold))
                            .ToList();
        }

        protected override GAGenerationalSelectionResult<GenomeType, GType, PType> 
                                                                    PerformGenerationalSelection()
        {
            var result = new GAGenerationalSelectionResult<GenomeType, GType, PType>();
            var currentPopulationSize = Population.Count;

            //Registers the genome copies which will be used to create children through mutation.
            Action<IList<GenomeType>, int> AddMutants = 
                delegate(IList<GenomeType> breeders, int mutationAmount) 
                {
                    foreach (var i in Enumerable.Range(0, mutationAmount))
                    {
                        if(result.Count == currentPopulationSize) 
                        {
                            break;
                        }

                        result.ToMutate.Add((GenomeType)breeders.RandomSingle().Copy());
                    }
                };
            
            //Add the champions of the species.
            var totalAverageFitness = 0.0;

            foreach (var species in populationSpecies)
            {
                if (species.Count > SPECIES_CHAMPION_THRESHOLD)
                {
                    result.ToRetain.Add((GenomeType)species.Best.Copy());
                }

                if (species.CanBreed)
                {
                    totalAverageFitness += species.AverageFitness;
                }
            }

            var populationSizeAfterChampions = currentPopulationSize - result.ToRetain.Count;

            //For each species.
            foreach (var species in populationSpecies.Where(species => species.CanBreed))
            {
                //Provided we haven't reached the target population size.
                if(result.Count == currentPopulationSize) 
                {
                    break;
                }

                //The number of children this species will be allowed to produce.
                var breedingLimit = (int)Math.Ceiling(species.AverageFitness / 
                                                    totalAverageFitness * 
                                                    populationSizeAfterChampions);

                //The members of the species which will be used for breeding.
                var speciesBreeders = SelectBreeders(species.Members.Cast<GenomeType>());

                //The number of children will be produced through breeding.
                var crossoverAmount = (int)Math.Floor(breedingLimit * CrossoverRate);

                //The number of children will be produced through mutation.
                var mutationAmount = (int)Math.Ceiling(breedingLimit * (1 - CrossoverRate));

                //Add the parent pairs using random selection with replacement.
                foreach (var i in Enumerable.Range(0, crossoverAmount))
                {
                    if(result.Count == currentPopulationSize) 
                    {
                        break;
                    }

                    var parent = speciesBreeders.RandomSingle();
                    GenomeType partner = null;

                    if(Random.NextDouble() <= InterSpeciesMatingRate) 
                    {
                        partner = Population.RandomSingle();
                    } 
                    else 
                    { 
                        partner = speciesBreeders.RandomSingle();
                    }

                    result.ParentPairs.Add(Tuple.Create(parent, partner));
                }

                //Add the genome copies which will be used to create children through mutation.
                AddMutants(speciesBreeders, mutationAmount);
            }

            //Return the selection.
            return result;
        }

        /// <summary>
        /// The rate at which children are generated using crossover and mutation, instead of just 
        /// mutation.
        /// </summary>
        public double CrossoverRate { get; set; }
    }
}
