using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA 
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

        void Update();

        void UpdateGenomes();
    }
}
