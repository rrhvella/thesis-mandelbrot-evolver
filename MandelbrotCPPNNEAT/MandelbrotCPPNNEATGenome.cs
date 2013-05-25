using CPPNNEAT.CPPNNEAT;
using CPPNNEAT.GeneticAlgorithms;

namespace MandelbrotCPPNNEAT
{
    public class MandelbrotCPPNNEATGenome : CPPNNEATGenome<CPPNNEATGeneCollection,
                                                MandelbrotCPPNNEATPhenome>
    {
        public MandelbrotCPPNNEATGenome()
            : base()
        {
        }

        public MandelbrotCPPNNEATGenome(MandelbrotCPPNNEATGenome parent, MandelbrotCPPNNEATGenome partner)
            : base(parent, partner)
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
            return new MandelbrotCPPNNEATGenome((MandelbrotCPPNNEATGenome)this, (MandelbrotCPPNNEATGenome)partner);
        }
    }
}