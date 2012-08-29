using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class GA<T> 
    {
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
    }
}
