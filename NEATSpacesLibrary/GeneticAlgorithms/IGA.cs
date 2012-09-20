using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA
    {
        Random Random
        {
            get;
        }

        void Iterate();
        void Update();
    }
}
