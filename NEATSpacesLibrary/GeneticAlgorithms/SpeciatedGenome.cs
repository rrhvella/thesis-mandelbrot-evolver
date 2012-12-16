using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    /// <summary>
    /// Base class for genomes in a speciated genetic algorithm.
    /// </summary>
    /// <typeparam name="GType">The type of the genome genetic sequence.</typeparam>
    /// <typeparam name="PType">The type of the phenome.</typeparam>
    public abstract class SpeciatedGenome<GType, PType> : Genome<GType, PType> 
    {
        /// <summary>
        /// The score of genome adjusted according to the size of the species.
        /// </summary>
        public double AdjustedScore
        {
            get
            {
                return Score / Species.Count;
            }
        }

        /// <summary>
        /// The species this genome belongs to.
        /// </summary>
        public Species<GType, PType> Species;
        
        /// <summary>
        /// Returns the compatibility distance between this genome and the given genome.
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public abstract double CompatibilityDistance(SpeciatedGenome<GType, PType> genome);

        public override void Update()
        {
            if (Species != null)
            {
                Species.Update();
            }

            base.Update();
        }
    }
}
