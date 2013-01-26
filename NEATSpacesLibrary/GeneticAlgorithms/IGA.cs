using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPPNNEAT.GeneticAlgorithms
{
    /// <summary>
    /// This interface allows variables to have a BaseGA type without having to specify
    /// the type parameters.
    /// </summary>
    public interface IGA 
    {
        /// <summary>
        /// Is true if the GA can not perform anymore generations.
        /// </summary>
        bool Failed 
        { 
            get; 
        }

        /// <summary>
        /// The random number generator for the GA.
        /// </summary>
        Random Random
        {
            get;
        }

        /// <summary>
        /// Initialises the GA's population.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Marks the GA's caches as stale.
        /// </summary>
        void Update();

        /// <summary>
        /// Goes through all the genomes and updates their phenomes and scores if they are stale.
        /// </summary>
        void UpdateGenomes();
    }
}
