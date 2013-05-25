using ComplexCPPNNEAT;
using GeneticAlgorithms;

namespace MandelbrotCPPNNEAT
{
    public class MandelbrotCPPNNEATGenome : CPPNNEATGenome<MandelbrotCPPNNEATPhenome>
    {
        public MandelbrotCPPNNEATGenome(MandelbrotCPPNNEATGA parent)
            : base(parent)
        {
        }

        public MandelbrotCPPNNEATGenome(MandelbrotCPPNNEATGA parentGA, MandelbrotCPPNNEATGenome parent, MandelbrotCPPNNEATGenome partner)
            : base(parentGA, parent, partner)
        {
        }

        protected override MandelbrotCPPNNEATPhenome GetPhenome()
        {
            var parent = Parent as MandelbrotCPPNNEATGA;
            return new MandelbrotCPPNNEATPhenome(GetNetwork(), parent.ViewPosition, parent.ViewScale);
        }

        protected override Genome<CPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome>
                InnerCrossover(Genome<CPPNNEATGeneCollection, MandelbrotCPPNNEATPhenome> partner)
        {
            return new MandelbrotCPPNNEATGenome((MandelbrotCPPNNEATGA)Parent, (MandelbrotCPPNNEATGenome)this, (MandelbrotCPPNNEATGenome)partner);
        }
    }
}