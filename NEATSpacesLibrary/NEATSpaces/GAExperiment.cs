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
    public abstract class GAExperiment<GAType, GenomeType, GType, PType>: Form
        where GAType : BaseGA<GenomeType, GType, PType>
        where GenomeType : Genome<GType, PType>, new()
    {
        private const double MILLISECONDS_TO_HOUR_RATIO = 1.0 / (1000.0 * 60.0 * 60.0);

        private int numberOfGenerations;
        private int numberOfRuns;

        private int matingEventsPerGenerations;
        private int numberOfTicksToUpdateImage;

        private int populationSize;

        private PictureBox imageDisplay;

        private Stopwatch stopWatch;

        private int TotalTicks
        {
            get
            {
                return matingEventsPerGenerations * numberOfGenerations;
            }
        }


        private class InnerGAList: GAList<GAType, GenomeType, GType, PType>
        {
            private GAExperiment<GAType, GenomeType, GType, PType> parent;

            public InnerGAList(GAExperiment<GAType, GenomeType, GType, PType> parent,
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

        public GAExperiment(int numberOfGenerations, int numberOfRuns, int matingEventsPerGenerations,
                        int numberOfTicksToUpdateImage, int populationSize)
        {
            this.numberOfGenerations = numberOfGenerations;
            this.numberOfRuns = numberOfRuns;

            this.matingEventsPerGenerations = matingEventsPerGenerations;
            this.numberOfTicksToUpdateImage = numberOfTicksToUpdateImage;

            this.populationSize = populationSize;

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

        protected abstract GAType CreateGAListGA(int populationSize, Func<GenomeType, double> scoreFunction);
        protected abstract Image GenerateImage(PType phenome);
        protected abstract double ScoreFunction(GenomeType genome);

        private void PrintInfo(int generationIndex, int matingIndex, InnerGAList algorithmList)
        {
            Console.Clear();

            if (algorithmList.Failed)
            {
                Console.WriteLine("The algorithm has failed completely, please change the parameters and try again.");
                return;
            }

            Console.WriteLine(String.Format("{0} out of {1} generations completed.", generationIndex, numberOfGenerations));
            Console.WriteLine(String.Format("{0} out of {1} mating events completed for current generation.", matingIndex, matingEventsPerGenerations));

            Console.Write("Average fitness: ");
            Console.WriteLine(algorithmList.AlgorithmList.Where(ga => !ga.Failed).Select(algorithm => algorithm.AverageScore).Average());

            Console.Write("Number of failures: ");
            Console.WriteLine(algorithmList.NumberOfFailures);

            var elapsedTime = stopWatch.ElapsedMilliseconds;
            var totalTime = elapsedTime *  TotalTicks / 
                                            ((double)generationIndex * matingEventsPerGenerations + matingIndex);

            Console.WriteLine(String.Format("Estimated time remaining: {0:f2} hours", 
                                        (totalTime - elapsedTime) * MILLISECONDS_TO_HOUR_RATIO));
        }

        private void RunGA()
        {
            Console.WriteLine("Initialising...");
            stopWatch = Stopwatch.StartNew();

            var algorithmList = new InnerGAList(
                                        this,
                                        numberOfRuns, 
                                        populationSize, 
                                        ScoreFunction
                                );

            algorithmList.Initialise();
            var data = new double[numberOfRuns, numberOfGenerations];

            foreach (var generationIndex in Enumerable.Range(0, numberOfGenerations))
            {
                var matingEventIndex = 0;
                PrintInfo(generationIndex, matingEventIndex, algorithmList);

                while (matingEventIndex + numberOfTicksToUpdateImage < matingEventsPerGenerations)
                {
                    algorithmList.PerformIterations(numberOfTicksToUpdateImage);
                    matingEventIndex += numberOfTicksToUpdateImage;

                    if (algorithmList.Failed)
                    {
                        break;
                    }

                    PrintInfo(generationIndex, matingEventIndex, algorithmList);

                    imageDisplay.Image = GenerateImage(algorithmList.Best.Best.Phenome);
                }

                algorithmList.PerformIterations(matingEventsPerGenerations - matingEventIndex);

                if (algorithmList.Failed)
                {
                    break;
                }

                foreach (var k in Enumerable.Range(0, numberOfRuns))
                {
                    data[k, generationIndex] = algorithmList.AlgorithmList[k].AverageScore;
                }
            }

            PrintInfo(numberOfGenerations, matingEventsPerGenerations, algorithmList);

            GenerateImage(algorithmList.Best.Best.Phenome).Save("output.png", ImageFormat.Png);

            FileStream file = new FileStream("output.csv", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);

            foreach (var i in Enumerable.Range(0, numberOfRuns))
            {
                foreach (var j in Enumerable.Range(0, numberOfGenerations))
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
