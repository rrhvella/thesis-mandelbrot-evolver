using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGA<T> : SpeciatedGA<T>
    {
        private int populationSize;

        public CPPNNEATGA(int populationSize)
        {
            this.populationSize = populationSize;
        }
    }
}
