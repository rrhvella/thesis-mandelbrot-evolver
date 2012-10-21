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
using SharpNeat.Decoders.HyperNeat;

namespace SharpNEATSpaces
{

    public partial class NEATSpaces : Form
    {
        private static readonly int NUMBER_OF_INPUTS = 2;
        private static readonly int NUMBER_OF_CRITICAL_POSITIONS = 2 + MapConstants.CHECKPOINTS.Count();
        private const double INPUT_DIVISOR = 1;
        private const int NUMBER_OF_THREADS = 10;
        private const int POPULATION_SIZE = 120;
        private const uint NUMBER_OF_GENERATIONS_BETWEEN_UPDATES = 1;
        private const double NON_CRITICAL_POSITION_ACTIVATION = -0.5;
        private const double CRITICAL_POSITION_ACTIVATION = 0.5;

        public class MapEvaluator : IPhenomeEvaluator<IBlackBox>
        {
            private ulong evaluationCount;

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

            public FitnessInfo Evaluate(IBlackBox phenome)
            {
                evaluationCount++;
                var fitness = GetMap(phenome).DistanceFromStartToEnd;

                return new FitnessInfo(fitness, fitness);
            }

            private static int GetIndex(int x, int y)
            {
                return x + y * MapConstants.MAP_SIZE;
            }

            public void Reset()
            {
                evaluationCount = 0;
            }

            public static Map GetMap(IBlackBox phenome)
            {
                var result = MapConstants.CreateMap();

                foreach (var i in Enumerable.Range(0, MapConstants.AREA))
                {
                    phenome.InputSignalArray[i] = NON_CRITICAL_POSITION_ACTIVATION;
                }

                phenome.InputSignalArray[GetIndex(MapConstants.START_NODE.X, MapConstants.START_NODE.Y)] = CRITICAL_POSITION_ACTIVATION;
                phenome.InputSignalArray[GetIndex(MapConstants.END_NODE.X, MapConstants.END_NODE.Y)] = CRITICAL_POSITION_ACTIVATION;

                foreach (var checkPoints in MapConstants.CHECKPOINTS)
                {
                    phenome.InputSignalArray[GetIndex(checkPoints.X, checkPoints.Y)] = CRITICAL_POSITION_ACTIVATION;
                }
                
                phenome.Activate();

                foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                    {
                        result[x, y] = phenome.OutputSignalArray[x + y * result.Width] < 0;
                    }
                }

                return result;
            }
        }

        public NEATSpaces()
        {
            InitializeComponent();

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
                                                                            4.0,
                                                                            2.0,
                                                                            0.0
                                                                        )
                                                                    ),
                                                               new NullComplexityRegulationStrategy());

            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = NUMBER_OF_THREADS;

            var inputNodes = new SubstrateNodeSet();
            var hiddenNodes = new SubstrateNodeSet();
            var outputNodes = new SubstrateNodeSet();

            uint inputID = 1;
            uint hiddenID = (uint)MapConstants.AREA + inputID;
            uint outputID = (uint)MapConstants.AREA + hiddenID;

            var nodeWidth = 1.0 / MapConstants.MAP_SIZE;
            var halfNodeWidth = nodeWidth / 2 - MapConstants.MAP_SIZE / 2.0;

            foreach (var x in Enumerable.Range(0, MapConstants.MAP_SIZE))
            {
                foreach (var y in Enumerable.Range(0, MapConstants.MAP_SIZE))
                {
                    var nodeCoord = GetNodeCoord(halfNodeWidth, x, y, nodeWidth);

                    inputNodes.NodeList.Add(new SubstrateNode(inputID++, nodeCoord)); 
                    hiddenNodes.NodeList.Add(new SubstrateNode(hiddenID++, nodeCoord)); 
                    outputNodes.NodeList.Add(new SubstrateNode(outputID++, nodeCoord)); 
                }
            }

            algorithm.Initialize(new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(
                                            new HyperNeatDecoder (
                                                new Substrate(
                                                    new List<SubstrateNodeSet> { 
                                                        inputNodes,
                                                        hiddenNodes,
                                                        outputNodes
                                                    }, 
                                                    DefaultActivationFunctionLibrary.CreateLibraryNeat(new BipolarSigmoid()),
                                                    0,
                                                    0.3,
                                                    8,
                                                    new List<NodeSetMapping> 
                                                    { 
                                                        new NodeSetMapping(0, 1, new DefaultNodeSetMappingFunction(null, false)),
                                                        new NodeSetMapping(1, 2, new DefaultNodeSetMappingFunction(null, false))
                                                    }
                                                ),
                                                NetworkActivationScheme.CreateAcyclicScheme(),
                                                NetworkActivationScheme.CreateAcyclicScheme()
                                            ),
                                            new MapEvaluator(),
                                            parallelOptions,
                                            true),
                                            new CppnGenomeFactory(NUMBER_OF_INPUTS * 2, 2,
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

        private double[] GetNodeCoord(double halfNodeWidth, double x, double y, double nodeWidth) 
        {
            return new double[] { halfNodeWidth + x * nodeWidth, halfNodeWidth + y * nodeWidth}; 
        }

    }
}
