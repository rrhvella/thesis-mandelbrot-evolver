using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA : IDebugabble
    {
        bool Failed 
        { 
            get; 
        }

        Random Random
        {
            get;
        }

        void Initialise();

        void SteadyStateIterate();
        void Update();

        void UpdateGenomes();
    }
}
