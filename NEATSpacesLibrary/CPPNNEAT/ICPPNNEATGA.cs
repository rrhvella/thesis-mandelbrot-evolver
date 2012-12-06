﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public interface ICPPNNEATGA : IGA
    {
        IList<CPPNNEATLinkGene> DefaultLinkGenes { get; }

        bool FeedForwardOnly { get;}

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
    }
}