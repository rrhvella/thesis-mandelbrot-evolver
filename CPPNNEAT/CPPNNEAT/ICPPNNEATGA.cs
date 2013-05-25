using System.Collections.Generic;
using System.Numerics;
using CPPNNEAT.GeneticAlgorithms;

namespace CPPNNEAT.CPPNNEAT
{
    public interface ICPPNNEATGA : ISpeciatedGA
    {
        IList<CPPNNEATLinkGene> DefaultLinkGenes { get; }

        bool FeedForwardOnly { get; }

        int GetEdgeInnovationNumber(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to);

        Complex GetRandomWeight();

        CPPNNEATNeuronGene GetHiddenNeuron(int linkIndex);

        double WeightMutationRate { get; set; }

        double MaxPerturbation { get; set; }

        double WeightPertubationRate { get; set; }

        double NewNeuronRate { get; set; }

        double NewLinkRate { get; set; }

        double MateByAveragingRate { get; set; }

        double MatchingGenesWeight { get; set; }

        double DisjointGenesWeight { get; set; }

        double ExcessGenesWeight { get; set; }

        double DisableGeneRate { get; set; }

        double FunctionDifferenceWeight { get; set; }
    }
}