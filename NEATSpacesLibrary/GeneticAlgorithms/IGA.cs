using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface IGA
    {
        double CrossoverRate { get; set; }
        double MutationRate { get; set; }

        void Iterate();

        void Update();
    }
}
