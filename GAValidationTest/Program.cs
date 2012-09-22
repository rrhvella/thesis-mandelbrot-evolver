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

namespace GAValidationTest
{
    public class Program: Form
    {
        private const int NUMBER_OF_GENERATIONS = 250;
        private const int NUMBER_OF_RUNS = 30;

        private const int MATING_EVENTS_PER_GENERATION = 2000;

        private const int POPULATION_SIZE = 120;
        private const double MUTATION_RATE = 0.01;
        private const double CROSSOVER_RATE = 0.05;

        private PictureBox imageDisplay;

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        public void Run()
        {
            Text = "Output of best genome from best GA";

            imageDisplay = new PictureBox();

            imageDisplay.Location = new Point(0, 0);
            imageDisplay.Size = new Size(Width, Height);
            imageDisplay.SizeMode = PictureBoxSizeMode.StretchImage;

            Controls.Add(imageDisplay);

            var thread = new Thread(RunGA);
            thread.Start();

            Application.Run(this);
        }


        private void PrintInfo(int i)
        {
            Console.Clear();
            Console.WriteLine(String.Format("{0} out of {1} generations completed.", i, NUMBER_OF_GENERATIONS));
        }

        private void RunGA()
        {
            var algorithmList = new GAList(NUMBER_OF_RUNS, POPULATION_SIZE, genome => genome.GeneCollection.DistanceFromStartToEnd);
            var data = new double[NUMBER_OF_GENERATIONS, NUMBER_OF_RUNS];

            foreach (var i in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
            {
                PrintInfo(i);

                algorithmList.PerformIterations(MATING_EVENTS_PER_GENERATION);

                imageDisplay.Image = algorithmList.Best.Best.GeneCollection.Image;

                foreach (var k in Enumerable.Range(0, NUMBER_OF_RUNS))
                {
                    data[i, k] = algorithmList.AlgorithmList[k].Best.Score;
                }
            }

            algorithmList.Best.Best.GeneCollection.Image.Save("output.png", ImageFormat.Png);

            FileStream file = new FileStream("output.csv", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);

            foreach (var i in Enumerable.Range(0, NUMBER_OF_RUNS))
            {
                foreach (var j in Enumerable.Range(0, NUMBER_OF_GENERATIONS))
                {
                    writer.WriteLine(String.Format("{0},{1},{2}", i, j, data[i,j]));
                }
            }

            writer.Close();
            file.Close();
        }
    }
}
