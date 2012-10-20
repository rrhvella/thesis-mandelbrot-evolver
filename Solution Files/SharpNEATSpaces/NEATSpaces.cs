using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes;
using SharpNeat.Genomes.Neat;
using SharpNeat.Genomes.HyperNeat;
using SharpNeat.SpeciationStrategies;
using SharpNeat.Utility;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.DistanceMetrics;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using NEATSpacesLibrary.NEATSpaces;
using SharpNeat.Decoders.Neat;
using SharpNeat.Decoders;
using System.Threading.Tasks;
using SharpNeat.Network;

namespace SharpNEATSpaces
{

    public partial class NEATSpaces : Form
    {
        private static readonly int NUMBER_OF_INPUTS = 2;
        private static readonly int NUMBER_OF_CRITICAL_POSITIONS = 2 + MapConstants.CHECKPOINTS.Count();
        private const double INPUT_DIVISOR = 1;
        private const int NUMBER_OF_THREADS = 10;
        private const int POPULATION_SIZE = 120;
        private const uint NUMBER_OF_GENERATIONS_BETWEEN_UPDATES = 10;

        public class MapEvaluator : IPhenomeEvaluator<IBlackBox>
        {
            private ulong evaluationCount;
            private double[][] euclideanDistanceCache;

            public ulong EvaluationCount
            {
                get
                {
                    return evaluationCount;
                }
            }

            public bool StopConditionSatisfied
            {
                get 
                {
                    return false;
                }
            }

            public MapEvaluator()
            {
            }

            public MapEvaluator(double[][] euclideanDistanceCache)
            {
                this.euclideanDistanceCache = euclideanDistanceCache;
            }

            public FitnessInfo Evaluate(IBlackBox phenome)
            {
                var result = MapConstants.CreateMap();

                foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                    {
                        var current = new MapNode(x, y);
                        var cacheRecord = euclideanDistanceCache[y * MapConstants.MAP_SIZE + x];

                        var input = new double[NUMBER_OF_INPUTS + NUMBER_OF_CRITICAL_POSITIONS];

                        phenome.InputSignalArray[0] = x / INPUT_DIVISOR;
                        phenome.InputSignalArray[1] = y / INPUT_DIVISOR;

                        foreach(var i in Enumerable.Range(0, NUMBER_OF_CRITICAL_POSITIONS))
                        {
                            phenome.InputSignalArray[i + NUMBER_OF_INPUTS] = cacheRecord[i];
                        }

                        phenome.Activate();
                        result[x, y] = phenome.OutputSignalArray[0] > 0.5;
                    }
                }
                
                evaluationCount++;
                var fitness = result.DistanceFromStartToEnd;

                return new FitnessInfo(fitness, fitness);
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        private double[][] euclideanDistanceCache;

        public NEATSpaces()
        {
            InitializeComponent();

            euclideanDistanceCache = new double[MapConstants.MAP_SIZE * MapConstants.MAP_SIZE][];

            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    var current = new MapNode(x, y);
                    var cacheRecord = new double[NUMBER_OF_CRITICAL_POSITIONS];

                    cacheRecord[0] = (current - MapConstants.START_NODE).EuclideanDistance / INPUT_DIVISOR;
                    cacheRecord[1] = (current - MapConstants.END_NODE).EuclideanDistance / INPUT_DIVISOR;

                    var i = 2;
                    foreach (var checkpoint in MapConstants.CHECKPOINTS)
                    {
                        cacheRecord[i++] = (current - checkpoint).EuclideanDistance / INPUT_DIVISOR;
                    }

                    euclideanDistanceCache[y * MapConstants.MAP_SIZE + x] = cacheRecord;
                }
            }

            Run();
        }

        private void Run()
        {
            var genomeParams = new NeatGenomeParameters();

            genomeParams.FeedforwardOnly = true;
            genomeParams.AddConnectionMutationProbability = 0.1;
            genomeParams.AddNodeMutationProbability = 0.03;
            genomeParams.ConnectionWeightMutationProbability = 0.9;
            genomeParams.ConnectionWeightRange = 3;
            genomeParams.DeleteConnectionMutationProbability = 0.01;

            var parameters = new NeatEvolutionAlgorithmParameters();

            parameters.InterspeciesMatingProportion = 0.01;
            parameters.SpecieCount = 15;
            parameters.ElitismProportion = 0.2;
            parameters.SelectionProportion = 0.2;
            parameters.OffspringAsexualProportion = 0.5;
            parameters.OffspringSexualProportion = 0.5;

            var algorithm = new NeatEvolutionAlgorithm<NeatGenome>(parameters,
                                                               new KMeansClusteringStrategy<NeatGenome>(
                                                                       new ManhattanDistanceMetric(
                                                                            2.0,
                                                                            2.0,
                                                                            10.0
                                                                        )
                                                                    ),
                                                               new NullComplexityRegulationStrategy());

            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = NUMBER_OF_THREADS;

            algorithm.Initialize(new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(
                                            new NeatGenomeDecoder
                                            (
                                                    NetworkActivationScheme.CreateAcyclicScheme()
                                            ),
                                            new MapEvaluator(euclideanDistanceCache),
                                            parallelOptions,
                                            true),
                                            new CppnGenomeFactory(NUMBER_OF_INPUTS + NUMBER_OF_CRITICAL_POSITIONS, 1,
                                                            DefaultActivationFunctionLibrary.CreateLibraryCppn(),
                                                            genomeParams),
                                            POPULATION_SIZE
                                            );

            algorithm.StartContinue();
            algorithm.UpdateScheme = new UpdateScheme(NUMBER_OF_GENERATIONS_BETWEEN_UPDATES);

            algorithm.UpdateEvent += new EventHandler(algorithm_UpdateEvent);
        }

        void algorithm_UpdateEvent(object sender, EventArgs e)
        {
            var evolutionaryAlgorithm = (NeatEvolutionAlgorithm<NeatGenome>)sender;

            Console.WriteLine(evolutionaryAlgorithm.CurrentGeneration + ": " + evolutionaryAlgorithm.Statistics._maxFitness);
        }
    }
}
