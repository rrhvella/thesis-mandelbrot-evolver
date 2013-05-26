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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotNetExtensions;
using GeneticAlgorithms;

namespace ComplexCPPNNEAT
{
    public abstract class BaseCPPNNEATGA<GenomeType, PType> :
BaseSpeciatedGA<GenomeType, CPPNNEATGeneCollection, PType>
        where GenomeType : CPPNNEATGenome<PType>
    {
        private Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int> edgeInnovationNumberMap;
        private Dictionary<int, Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>> edgeMap;
        private Dictionary<int, CPPNNEATNeuronGene> hiddenNeuronMap;

        public IList<Func<CPPNNEATActivationFunction>> CanonicalFunctionList
        {
            get;
            private set;
        }

        public BaseCPPNNEATGA(int numberOfInputs, int populationSize, Func<GenomeType, double> scoreFunction,
List<Func<CPPNNEATActivationFunction>> canonicalFunctionList,
bool feedForwardOnly)
            : this(numberOfInputs, populationSize, scoreFunction, canonicalFunctionList, null, feedForwardOnly)
        {
        }

        public BaseCPPNNEATGA(int numberOfInputs, int populationSize, Func<GenomeType, double> scoreFunction,
List<Func<CPPNNEATActivationFunction>> canonicalFunctionList,
Func<CPPNNEATActivationFunction> outputActivationFunction,
bool feedForwardOnly)
            : base(populationSize, scoreFunction)
        {
            if (numberOfInputs < 1)
            {
                throw new ApplicationException("Numbers of inputs cannot be less than 1.");
            }

            this.NumberOfInputs = numberOfInputs;
            this.CanonicalFunctionList = canonicalFunctionList;

            this.edgeInnovationNumberMap = new Dictionary<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>, int>();
            this.hiddenNeuronMap = new Dictionary<int, CPPNNEATNeuronGene>();
            this.edgeMap = new Dictionary<int, Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>>();

            this.defaultNeuronGenes = new List<CPPNNEATNeuronGene>();
            this.defaultLinkGenes = new List<CPPNNEATLinkGene>();

            this.FeedForwardOnly = feedForwardOnly;

            this.IterationComplete += new EventHandler<IterationEventArgs>(CPPNNEATGA_IterationComplete);

            var outputGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 1, CPPNNeuronType.Output,
                                        (outputActivationFunction == null) ?
                                            canonicalFunctionList.RandomSingle()() : outputActivationFunction());

            var currentGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 0, CPPNNeuronType.Bias, null);
            defaultNeuronGenes.Add(currentGene);
            defaultLinkGenes.Add(new CPPNNEATLinkGene(GetEdgeInnovationNumber(currentGene, outputGene), currentGene,
                                                        outputGene, 0));

            foreach (var i in Enumerable.Range(0, numberOfInputs))
            {
                currentGene = new CPPNNEATNeuronGene(neuronInnovationNumber++, 0, CPPNNeuronType.Input, null);

                defaultNeuronGenes.Add(currentGene);
                defaultLinkGenes.Add(new CPPNNEATLinkGene(GetEdgeInnovationNumber(currentGene, outputGene), currentGene,
                                                            outputGene, 0));
            }

            defaultNeuronGenes.Add(outputGene);
        }

        public int NumberOfInputs
        {
            get;
            private set;
        }

        public double WeightMutationRate
        {
            get;
            set;
        }

        public double NewNeuronRate
        {
            get;
            set;
        }

        public double NewLinkRate
        {
            get;
            set;
        }

        public double DisableGeneRate
        {
            get;
            set;
        }

        public double WeightPertubationRate
        {
            get;
            set;
        }

        public double MateByAveragingRate
        {
            get;
            set;
        }

        public double MaxPerturbation
        {
            get;
            set;
        }

        public double MaxWeight
        {
            get;
            set;
        }

        public double ExcessGenesWeight
        {
            get;
            set;
        }

        public double DisjointGenesWeight
        {
            get;
            set;
        }

        public double MatchingGenesWeight
        {
            get;
            set;
        }

        public double FunctionDifferenceWeight
        {
            get;
            set;
        }

        public int IterationsToClearLinkCache
        {
            get;
            set;
        }

        private List<CPPNNEATNeuronGene> defaultNeuronGenes;

        public IList<CPPNNEATNeuronGene> DefaultNeuronGenes
        {
            get
            {
                return defaultNeuronGenes.AsReadOnly();
            }
            private set
            {
                defaultNeuronGenes = (List<CPPNNEATNeuronGene>)value;
            }
        }

        private List<CPPNNEATLinkGene> defaultLinkGenes;

        public IList<CPPNNEATLinkGene> DefaultLinkGenes
        {
            get
            {
                return defaultLinkGenes.AsReadOnly();
            }
            private set
            {
                defaultLinkGenes = (List<CPPNNEATLinkGene>)value;
            }
        }

        public Complex GetRandomWeight()
        {
            return MathExtensions.ComplexRandom(-MaxWeight, MaxWeight);
        }

        private int edgeInnovationNumber = 0;

        private int neuronInnovationNumber = 0;

        public bool FeedForwardOnly
        {
            get;
            private set;
        }

        public void CPPNNEATGA_IterationComplete(object sender, IterationEventArgs e)
        {
            if (IterationsToClearLinkCache != 0 && e.IterationNumber % IterationsToClearLinkCache == 0)
            {
                edgeInnovationNumberMap.Clear();
            }
        }

        public int GetEdgeInnovationNumber(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            var key = new Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>(from, to);

            if (!edgeInnovationNumberMap.ContainsKey(key))
            {
                edgeInnovationNumberMap[key] = edgeInnovationNumber;
                edgeMap[edgeInnovationNumber] = key;

                edgeInnovationNumber++;
            }

            return edgeInnovationNumberMap[key];
        }

        public CPPNNEATNeuronGene GetHiddenNeuron(int innovationNumber)
        {
            if (!hiddenNeuronMap.ContainsKey(innovationNumber))
            {
                var edge = edgeMap[innovationNumber];
                var level = (edge.Item1.Level + edge.Item2.Level) / 2;

                hiddenNeuronMap[innovationNumber] = new CPPNNEATNeuronGene(neuronInnovationNumber++, level, CPPNNeuronType.Hidden,
                                                                    CanonicalFunctionList.RandomSingle()());
            }

            return hiddenNeuronMap[innovationNumber];
        }
    }
}