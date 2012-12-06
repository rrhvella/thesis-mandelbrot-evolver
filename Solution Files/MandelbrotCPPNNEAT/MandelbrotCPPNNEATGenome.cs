using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;
using NEATSpacesLibrary.GeneticAlgorithms;
using NEATSpacesLibrary.Extensions;
using MandelbrotCPPNNEAT;

namespace MandelbrotCPPNNEAT
{
    public class MandelbrotCPPNNEATGeneCollection: CPPNNEATGeneCollection 
    {
        public Complex ViewPosition;
        public double ViewScale;
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
}
