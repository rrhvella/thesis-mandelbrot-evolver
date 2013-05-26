/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System.Collections.Generic;
using System.Numerics;
using GeneticAlgorithms;

namespace ComplexCPPNNEAT
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