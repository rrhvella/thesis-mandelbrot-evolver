using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class GA<T> 
    {
        private int p;

        public GA(int p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
        public T Best
        {
            get
            {
                return default(T);
            }
        }

        public void Iterate()
        {
            throw new NotImplementedException();
        }

        public double CrossoverRate { get; set; }

        public double MutationRate { get; set; }
    }
}
