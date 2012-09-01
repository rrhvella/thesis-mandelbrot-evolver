using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.GeneticAlgorithms
{
    public class SpeciatedGA<T> : GA<T>
    {
        private int p;

        public double InterSpeciesMatingRate {
            get{
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public double CompatibilityDistanceThreshold {
            get{
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public int NoInnovationThreshold {
            get{
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public SpeciatedGA(int p): base(p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}
