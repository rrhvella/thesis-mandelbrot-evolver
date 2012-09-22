using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA : IDebugabble
    {
        Random Random
        {
            get;
        }

        void SteadyStateIterate();
        void Update();

        void UpdateGenomes();
    }
}
