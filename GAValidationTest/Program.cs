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
    public class Program: MapForm<SteadyStateGA<MapGenome, bool[], bool[]>, MapGenome, bool[], bool[]>
    {
        protected override Map TransformPhenome(bool[] phenome)
        {
            return new Map(phenome, MapConstants.MAP_SIZE, MapConstants.START_NODE, MapConstants.END_NODE,
                        MapConstants.CHECKPOINTS, MapConstants.MANDATORY_CHECKPOINT_LEVEL);
        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override SteadyStateGA<MapGenome, bool[], bool[]> CreateGAListGA(int populationSize, 
                                                                            Func<MapGenome, double> scoreFunction)
        {
            return new SteadyStateGA<MapGenome, bool[], bool[]>(populationSize, scoreFunction);
        }
    }
}
