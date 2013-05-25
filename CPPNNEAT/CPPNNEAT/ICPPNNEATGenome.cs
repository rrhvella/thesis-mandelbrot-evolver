using CPPNNEAT.GeneticAlgorithms;

namespace CPPNNEAT.CPPNNEAT
{
    public interface ICPPNNEATGenome
    {
        void Update();

        IGA Parent { get; }
    }
}