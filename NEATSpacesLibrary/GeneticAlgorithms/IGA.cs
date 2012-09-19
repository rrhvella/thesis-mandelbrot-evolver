using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA
    {
        void Iterate();
        void Update();
    }
}
