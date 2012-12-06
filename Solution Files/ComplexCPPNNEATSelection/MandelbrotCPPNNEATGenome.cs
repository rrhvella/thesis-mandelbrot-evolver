using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NEATSpacesLibrary.Extensions;

namespace ComplexCPPNNEATSelection
{
    public class MandelbrotCPPNNEATGeneCollection: CPPNNEATGeneCollection 
    {
        public Complex ViewPosition;
        public double ViewScale;
    }

    public class MandelbrotCPPNNEATPhenome 
    {
        private const double ESCAPE_MAGNITUDE = 2;

        private const int ALPHA_OFFSET = 24;
        private const int RED_OFFSET = 16;
        private const int GREEN_OFFSET = 8;
        private const int BLUE_OFFSET = 0;

        private const int BYTES_PER_INT = 4;

        private static readonly Color[] BASE_COLOURS = new Color[] { 
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(0, 255, 0)
        };


        private CPPNNetwork network;
        private Complex viewPosition;
        private double viewScale;

        public MandelbrotCPPNNEATPhenome(CPPNNetwork viewNetwork, Complex viewPosition, double viewScale)
        {
            this.network = viewNetwork;
            this.viewPosition = viewPosition;
            this.viewScale = viewScale;
        }

        public Bitmap GetImage(int ViewWidth, int ViewHeight, int iterationNumberLimit)
        {
            Bitmap fractalImage = new Bitmap(ViewWidth, ViewHeight);
            network.Reset();

            var drawingBuffer = fractalImage.LockBits(new Rectangle(0, 0, ViewWidth, ViewHeight),
                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            foreach (var x in Enumerable.Range(0, ViewWidth))
            {
                foreach (var y in Enumerable.Range(0, ViewHeight))
                {
                    var positionComplex = viewPosition + 
                                            (new Complex((double)x / ViewWidth, (double)y / ViewHeight) * viewScale);

                    var complex = Complex.Zero;
                    var currentMagnitude = complex.Magnitude;

                    int i = 0;

                    for (; i < iterationNumberLimit && currentMagnitude < ESCAPE_MAGNITUDE; i++)
                    {
                        complex = network.GetActivation(new Complex[] { positionComplex, complex });
                        currentMagnitude = complex.Magnitude;

                    }

                    Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * y + x * BYTES_PER_INT, 
                                    ToColour(i, iterationNumberLimit));

                }
            }

            fractalImage.UnlockBits(drawingBuffer);

            return fractalImage;
        }

        private int ToColour(int iterationNumber, int iterationNumberLimit)
        {
            var normalisedIterationNumber = iterationNumber / (double)iterationNumberLimit;

            var brightness = 1 - normalisedIterationNumber;
            var baseColor = BASE_COLOURS[(int)Math.Round(normalisedIterationNumber * (BASE_COLOURS.Length - 1))];

            var r = (int)(baseColor.R * brightness);
            var g = (int)(baseColor.G * brightness);
            var b = (int)(baseColor.B * brightness);

            return 255 << ALPHA_OFFSET | r << RED_OFFSET | 
                    g << GREEN_OFFSET | b << BLUE_OFFSET;
        }

    }

    public class MandelbrotCPPNNEATGenome: CPPNNEATGenome<MandelbrotCPPNNEATGeneCollection,
                                                            MandelbrotCPPNNEATPhenome>
    {

        public override void Initialise()
        {
            base.Initialise();
            var parentGA = Parent as MandelbrotCPPNNEATGA;

            GeneCollection.ViewPosition = parentGA.InitialPositionOrigin +
                                            MathExtensions.ComplexRandom(parentGA.InitialPositionMaxDisplacement);

            GeneCollection.ViewScale = MathExtensions.RandomNumber(parentGA.MinInitialScale, parentGA.MaxInitialScale);
        }

        public MandelbrotCPPNNEATGenome(): base()
        {
        }

        public MandelbrotCPPNNEATGenome(MandelbrotCPPNNEATGenome parent, MandelbrotCPPNNEATGenome partner)
            :base(parent, partner)
        {
            GeneCollection.ViewPosition = (Parent.Random.NextDouble() < 0.5)? parent.GeneCollection.ViewPosition :
                                                                partner.GeneCollection.ViewPosition;

            GeneCollection.ViewScale = (Parent.Random.NextDouble() < 0.5)? parent.GeneCollection.ViewScale :
                                                                partner.GeneCollection.ViewScale;
        }

        protected override MandelbrotCPPNNEATPhenome GetPhenome()
        {
            var parent = Parent as MandelbrotCPPNNEATGA;
            return new MandelbrotCPPNNEATPhenome(GetNetwork(), GeneCollection.ViewPosition, GeneCollection.ViewScale);
        }

        public override double CompatibilityDistance(SpeciatedGenome<MandelbrotCPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> genome)
        {
            var parent = Parent as MandelbrotCPPNNEATGA;
            return base.CompatibilityDistance(genome) + 
                (genome.GeneCollection.ViewPosition - GeneCollection.ViewPosition).Magnitude * 
                    parent.ViewPositionDistanceCoefficient +
                Math.Abs(genome.GeneCollection.ViewScale - GeneCollection.ViewScale) * 
                    parent.ViewScaleDistanceCoefficient;
        }

        protected override Genome<MandelbrotCPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome>[] 
                InnerCrossover(Genome<MandelbrotCPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> partner)
        {
            return new[] {
                new MandelbrotCPPNNEATGenome((MandelbrotCPPNNEATGenome)this, (MandelbrotCPPNNEATGenome)partner),
                new MandelbrotCPPNNEATGenome((MandelbrotCPPNNEATGenome)this, (MandelbrotCPPNNEATGenome)partner)
            };
        }

        protected override void InnerMutate()
        {
         	base.InnerMutate();

            var parent = Parent as MandelbrotCPPNNEATGA;

            GeneCollection.ViewPosition += MathExtensions.ComplexRandom(parent.DisplacementStandardDeviation);
            GeneCollection.ViewScale *= Math.Exp(Math.Log(parent.ScaleTweakCoefficient)) * parent.Random.NextDouble();
        }

        public override Genome<MandelbrotCPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> InnerCopy()
        {
            var baseCopy = base.InnerCopy();

            baseCopy.GeneCollection.ViewPosition = this.GeneCollection.ViewPosition;
            baseCopy.GeneCollection.ViewScale = this.GeneCollection.ViewScale;

            return baseCopy;
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append("View position: ");
            result.AppendLine(GeneCollection.ViewPosition.ToString());

            result.Append("View scale: ");
            result.AppendLine(GeneCollection.ViewScale.ToString());

            result.AppendLine();

            result.Append(base.ToString());

            return result.ToString();
        }
    }

    public class MandelbrotCPPNNEATGA : BaseCPPNNEATGA<MandelbrotCPPNNEATGenome, MandelbrotCPPNNEATGeneCollection,
                                                        MandelbrotCPPNNEATPhenome>, ICPPNNEATGA
    {
        public Complex InitialPositionOrigin
        {
            get;
            set;
        }

        public double InitialPositionMaxDisplacement
        {
            get;
            set;
        }

        public double DisplacementStandardDeviation
        {
            get;
            set;
        }

        public double ScaleTweakCoefficient
        {
            get;
            set;
        }

        public double ExchangeProbability
        {
            get;
            set;
        }

        public double MinInitialScale
        {
            get;
            set;
        }

        public double MaxInitialScale
        {
            get;
            set;
        }

        public double ViewPositionDistanceCoefficient
        {
            get;
            set;
        }

        public double ViewScaleDistanceCoefficient
        {
            get;
            set;
        }

        public MandelbrotCPPNNEATGA(int numberOfInputs, int populationSize, 
                        Func<MandelbrotCPPNNEATGenome, double> scoreFunction,
                        List<Func<CPPNNEATActivationFunction>> canonicalFunctionList, 
                        Func<CPPNNEATActivationFunction> outputActivationFunction)
            : base(numberOfInputs, populationSize, scoreFunction, canonicalFunctionList, outputActivationFunction, true) 
        {
        }
    }
}
