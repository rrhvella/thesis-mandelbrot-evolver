using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// This interface allows variables to have a CPPNNEATGenome type without having to fill
    /// the type parameters.
    /// </summary>
    public interface ICPPNNEATGenome
    {
        /// <summary>
        /// Marks the genome for update.
        /// </summary>
        void Update();

        /// <summary>
        /// The genetic algorithm this genome belongs to.
        /// </summary>
        IGA Parent { get; }
    }
}
