using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.NEATSpaces;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace NEATSpacesLibrary.NEATSpaces
{
    public abstract class MapForm<GAType, GenomeType, GType, PType>: Form
        where GAType : BaseGA<GenomeType, GType, PType>
        where GenomeType : Genome<GType, PType>, new()
    {
        private const int NUMBER_OF_GENERATIONS = 250;
        private const int NUMBER_OF_RUNS = 30;

        private const int MATING_EVENTS_PER_GENERATION = 2000;
        private const int TOTAL_TICKS = MATING_EVENTS_PER_GENERATION * NUMBER_OF_GENERATIONS;

        private const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 100;

        private const int POPULATION_SIZE = 120;
        private double MILLISECONDS_TO_HOUR_RATIO = 1.0 / (1000.0 * 60.0 * 60.0);

        private Stopwatch stopWatch;

        private class InnerGAList: GAList<GAType, GenomeType, GType, PType>
        {
            private MapForm<GAType, GenomeType, GType, PType> parent;

            public InnerGAList(MapForm<GAType, GenomeType, GType, PType> parent,
                int numberOfReplicants, int populationSize, Func<GenomeType, double> scoreFunction) :
                base(numberOfReplicants, populationSize, scoreFunction)
            {
                this.parent = parent;
            }

            public override GAType CreateGA(int populationSize, Func<GenomeType, double> scoreFunction)
            {
                return parent.CreateGAListGA(populationSize, scoreFunction);
            }
        }

        private PictureBox imageDisplay;

        public MapForm()
        {
            Text = "Output of best genome from best GA";
            ClientSize = new Size(Width, Width);

            imageDisplay = new PictureBox();

            imageDisplay.Location = new Point(0, 0);
            imageDisplay.Size = ClientSize;
            imageDisplay.SizeMode = PictureBoxSizeMode.StretchImage;

            Controls.Add(imageDisplay);
        }

        public void Run()
        {
            var thread = new Thread(RunGA);
            thread.Start();

            Application.Run(this);
        }

        protected abstract Map TransformPhenome(PType phenome);
        protected abstract GAType CreateGAListGA(int populationSize, Func<GenomeType, double> scoreFunction);

        private void PrintInfo(int generationIndex, int matingIndex, InnerGAList algorithmList)
        {
            Console.Clear();

            if (algorithmList.Failed)
            {
                Console.WriteLine("The algorithm has failed completely, please change the parameters and try again.");
                return;
            }

            Console.WriteLine(String.Format("{0} out of {1} generations completed.", generationIndex, NUMBER_OF_GENERATIONS));
            Console.WriteLine(String.Format("{0} out of {1} mating events completed for current generation.", matingIndex, MATING_EVENTS_PER_GENERATION));

            Console.Write("Average fitness: ");
            Console.WriteLine(algorithmList.AlgorithmList.Select(algorithm => algorithm.AverageScore).Average());

            Console.Write("Number of failures: ");
            Console.WriteLine(algorithmList.NumberOfFailures);

            var elapsedTime = stopWatch.ElapsedMilliseconds;
            var totalTime = elapsedTime *  TOTAL_TICKS / 
                                            ((double)generationIndex * MATING_EVENTS_PER_GENERATION + matingIndex);

            Console.WriteLine(String.Format("Estimated time remaining: {0:f2} hours", 
                                        (totalTime - elapsedTime) * MILLISECONDS_TO_HOUR_RATIO));
        }

        private void RunGA()
        {
            Console.WriteLine("Initialising...");
            stopWatch = Stopwatch.StartNew();

            var algorithmList = new InnerGAList(
                                        this,
                                        NUMBER_OF_RUNS, 
                                        POPULATION_SIZE, 
                                        genome => TransformPhenome(genome.Phenome).DistanceFromStartToEnd
                                );

            algorithmList.Initialise();
            var data = new double[NUMBER_OF_RUNS, NUMBER_OF_GENERATIONS];

            foreach (var generationIndex in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
            {
                var matingEventIndex = 0;
                PrintInfo(generationIndex, matingEventIndex, algorithmList);

                while (matingEventIndex + NUMBER_OF_TICKS_TO_UPDATE_IMAGE < MATING_EVENTS_PER_GENERATION)
                {
                    algorithmList.PerformIterations(NUMBER_OF_TICKS_TO_UPDATE_IMAGE);
                    matingEventIndex += NUMBER_OF_TICKS_TO_UPDATE_IMAGE;

                    if (algorithmList.Failed)
                    {
                        break;
                    }

                    var map = TransformPhenome(algorithmList.Best.Best.Phenome);

                    PrintInfo(generationIndex, matingEventIndex, algorithmList);

                    imageDisplay.Image = map.Image;
                }

                algorithmList.PerformIterations(MATING_EVENTS_PER_GENERATION - matingEventIndex);

                if (algorithmList.Failed)
                {
                    break;
                }

                foreach (var k in Enumerable.Range(0, NUMBER_OF_RUNS))
                {
                    data[k, generationIndex] = algorithmList.AlgorithmList[k].AverageScore;
                }
            }

            PrintInfo(NUMBER_OF_GENERATIONS, MATING_EVENTS_PER_GENERATION, algorithmList);

            var finalMap = TransformPhenome(algorithmList.Best.Best.Phenome);
            finalMap.Image.Save("output.png", ImageFormat.Png);

            FileStream file = new FileStream("output.csv", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);

            foreach (var i in Enumerable.Range(0, NUMBER_OF_RUNS))
            {
                foreach (var j in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
                {
                    writer.WriteLine(String.Format("{0},{1},{2}", i, j, data[i, j]));
                }
            }

            writer.Close();
            file.Close();

            Console.WriteLine();
            Console.WriteLine("Experiment finished... press any key to exit.");

            Console.ReadKey();

            Application.Exit();
        }
    }
}
