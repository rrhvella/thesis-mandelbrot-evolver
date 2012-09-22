using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace GAValidationTest
{
    public class GAList
    {
        private int populationSize;

        public List<SteadyStateGA<MapGenome, Map, Map>> algorithmList;
        public IList<SteadyStateGA<MapGenome, Map, Map>> AlgorithmList
        {
            get
            {
                return algorithmList.AsReadOnly();
            }
        }

        private bool bestCacheInvalidated;
        private SteadyStateGA<MapGenome, Map, Map> best;
        private int numberOfReplicants;
        public SteadyStateGA<MapGenome, Map, Map> Best
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

        public GAList(int numberOfReplicants, int populationSize, Func<MapGenome, double> scoreFunction)
        {
            this.numberOfReplicants = numberOfReplicants;
            this.populationSize = populationSize;
            this.algorithmList = new List<SteadyStateGA<MapGenome, Map, Map>>();

            foreach (var i in Enumerable.Range(0, numberOfReplicants))
            {
                var newGA = new SteadyStateGA<MapGenome, Map, Map>(populationSize, scoreFunction);
                newGA.Initialise();

                algorithmList.Add(newGA);
            }

            bestCacheInvalidated = true;
        }

        public void PerformIterations(int numberOfIterations)
        {
            Parallel.ForEach(algorithmList, 
                delegate(SteadyStateGA<MapGenome, Map, Map> ga)
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
