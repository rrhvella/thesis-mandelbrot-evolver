using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public interface ISpeciatedGA : IGA
    {
        double InterSpeciesMatingRate { get; set; }
        double CompatibilityDistanceThreshold { get; set; }
        int NoInnovationThreshold { get; set; }
    }
}
