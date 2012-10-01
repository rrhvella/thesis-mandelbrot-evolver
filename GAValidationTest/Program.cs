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
    public class Program: MapForm<SteadyStateGA<MapGenome, Map, Map>, MapGenome, Map, Map>
    {
        protected override Map TransformPhenome(Map phenome)
        {
            return phenome;
        }

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        protected override SteadyStateGA<MapGenome, Map, Map> CreateGAListGA(int populationSize, Func<MapGenome, double> scoreFunction)
        {
            return new SteadyStateGA<MapGenome, Map, Map>(populationSize, scoreFunction);
        }
    }
}
