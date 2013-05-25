using GeneticAlgorithms;

namespace ComplexCPPNNEAT
{
    public interface ICPPNNEATGenome
    {
        void Update();

        IGA Parent { get; }
    }
}