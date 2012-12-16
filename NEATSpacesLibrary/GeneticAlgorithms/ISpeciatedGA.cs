using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    /// <summary>
    /// This interface allows variables to have a BaseSpeciatedGA type without having to specify
    /// the type parameters.
    /// </summary>
    public interface ISpeciatedGA : IGA
    {
        /// <summary>
        /// The rate at which individuals mate with individuals outside of their species.
        /// </summary>
        double InterSpeciesMatingRate { get; set; }

        /// <summary>
        /// Determines the point at which an individual is not part of a species, based on its 
        /// distance to the species representative
        /// </summary>
        double CompatibilityDistanceThreshold { get; set; }

        /// <summary>
        /// Determines the point at which a species is considered stagnant, based on the number
        /// of iterations without an improvement in the best fitness.
        /// </summary>
        int NoInnovationThreshold { get; set; }
    }
}
