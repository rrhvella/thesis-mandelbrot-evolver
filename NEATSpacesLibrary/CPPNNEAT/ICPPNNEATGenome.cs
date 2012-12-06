using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public interface ICPPNNEATGenome
    {
        void Update();

        IGA Parent { get; }
    }
}
