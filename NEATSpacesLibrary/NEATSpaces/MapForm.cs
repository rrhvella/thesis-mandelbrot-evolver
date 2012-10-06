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

namespace NEATSpacesLibrary.NEATSpaces
{
    public abstract class MapForm<GAType, GenomeType, GType, PType>: Form
        where GAType : BaseGA<GenomeType, GType, PType>
        where GenomeType : Genome<GType, PType>, new()
    {
        private const int NUMBER_OF_GENERATIONS = 250;
        private const int NUMBER_OF_RUNS = 1;

        private const int MATING_EVENTS_PER_GENERATION = 2000;
        private const int NUMBER_OF_TICKS_TO_UPDATE_IMAGE = 100;

        private const int POPULATION_SIZE = 120;

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

        private void PrintInfo(int i, int j, InnerGAList algorithmList)
        {
            Console.Clear();
            Console.WriteLine(String.Format("{0} out of {1} generations completed.", i, NUMBER_OF_GENERATIONS));
            Console.WriteLine(String.Format("{0} out of {1} mating events completed for current generation.", j, MATING_EVENTS_PER_GENERATION));
            Console.Write("Average fitness: ");
            Console.WriteLine(algorithmList.AlgorithmList.Select(algorithm => algorithm.Best.Score).Average());
        }

        private void RunGA()
        {
            Console.WriteLine("Initialising...");

            var algorithmList = new InnerGAList(
                                        this,
                                        NUMBER_OF_RUNS, 
                                        POPULATION_SIZE, 
                                        genome => TransformPhenome(genome.Phenome).DistanceFromStartToEnd
                                );

            algorithmList.Initialise();
            var data = new double[NUMBER_OF_RUNS, NUMBER_OF_GENERATIONS];

            foreach (var i in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
            {
                var j = 0;
                PrintInfo(i, j, algorithmList);

                while (j + NUMBER_OF_TICKS_TO_UPDATE_IMAGE < MATING_EVENTS_PER_GENERATION)
                {
                    algorithmList.PerformIterations(NUMBER_OF_TICKS_TO_UPDATE_IMAGE);
                    j += NUMBER_OF_TICKS_TO_UPDATE_IMAGE;

                    var map = TransformPhenome(algorithmList.Best.Best.Phenome);

                    PrintInfo(i, j, algorithmList);

                    imageDisplay.Image = map.Image;
                }

                algorithmList.PerformIterations(MATING_EVENTS_PER_GENERATION - j);

                foreach (var k in Enumerable.Range(0, NUMBER_OF_RUNS))
                {
                    data[k, i] = algorithmList.AlgorithmList[k].Best.Score;
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
