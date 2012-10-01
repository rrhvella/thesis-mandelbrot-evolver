using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace NEATSpacesLibrary.NEATSpaces
{
    public abstract class GAList<GAType, GenomeType, GType, PType> 
        where GAType : BaseGA<GenomeType, GType, PType>
        where GenomeType : Genome<GType, PType>, new()
    {
        private int populationSize;

        public List<GAType> algorithmList;
        public IList<GAType> AlgorithmList
        {
            get
            {
                return algorithmList.AsReadOnly();
            }
        }

        private bool bestCacheInvalidated;
        private GAType best;
        private int numberOfReplicants;
        private Func<GenomeType, double> scoreFunction;
        public GAType Best
        {
            get
            {
                if (bestCacheInvalidated)
                {
                    best = algorithmList.MaxBy(ga => ga.Best.Score);
                    bestCacheInvalidated = false;
                }

                return best;
            }
        }

        public GAList(int numberOfReplicants, int populationSize, Func<GenomeType, double> scoreFunction)
        {
            this.numberOfReplicants = numberOfReplicants;
            this.populationSize = populationSize;
            this.algorithmList = new List<GAType>();
            this.scoreFunction = scoreFunction;
        }

        public void Initialise()
        {
            foreach (var i in Enumerable.Range(0, numberOfReplicants))
            {
                var newGA = CreateGA(populationSize, scoreFunction);
                newGA.Initialise();

                algorithmList.Add(newGA);
            }

            bestCacheInvalidated = true;
        }

        public abstract GAType CreateGA(int populationSize, Func<GenomeType, double> scoreFunction);

        public void PerformIterations(int numberOfIterations)
        {
            Parallel.ForEach(algorithmList, 
                delegate(GAType ga)
                {
                    foreach (var i in Enumerable.Range(0, numberOfIterations))
                    {
                        ga.SteadyStateIterate();
                    }
                }
            );

            bestCacheInvalidated = true;
        }
    }
}
