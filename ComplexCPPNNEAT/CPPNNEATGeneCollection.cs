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
using DotNetExtensions;

namespace ComplexCPPNNEAT
{
    public static class LinkGenesListExtensions
    {
        public static IEnumerable<CPPNNEATNeuronGene> Neurons(this IEnumerable<CPPNNEATLinkGene> self)
        {
            return self.Select(elem => elem.From).Union(self.Select(elem => elem.To))
                    .Distinct();
        }
    }

    public class CPPNNEATGeneCollection
    {
        private Dictionary<int, CPPNNEATLinkGene> linkGeneMap;

        public ICPPNNEATGenome Parent
        {
            get;
            private set;
        }

        public IEnumerable<CPPNNEATActivationFunction> ActivationFunctions
        {
            get
            {
                return neuronGeneSet.Select(neuronGene => neuronGene.ActivationFunction)
                                .Where(func => func != null);
            }
        }

        public ICPPNNEATGA ParentGA
        {
            get
            {
                return (ICPPNNEATGA)Parent.Parent;
            }
        }

        private bool sortedLinkGenesCacheExpired;

        private List<CPPNNEATLinkGene> sortedLinkGenes;

        private HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>> possibleConnections;

        private HashSet<CPPNNEATNeuronGene> orphanedNeurons;

        public IList<CPPNNEATLinkGene> LinkGenes
        {
            get
            {
                if (sortedLinkGenesCacheExpired)
                {
                    sortedLinkGenes = linkGeneMap.Values.ToList();
                    sortedLinkGenes.Sort(new Comparison<CPPNNEATLinkGene>(
                                delegate(CPPNNEATLinkGene first, CPPNNEATLinkGene second)
                                {
                                    return first.InnovationNumber.CompareTo(second.InnovationNumber);
                                }
                    ));

                    sortedLinkGenesCacheExpired = false;
                }

                return sortedLinkGenes.AsReadOnly();
            }
        }

        private ISet<CPPNNEATNeuronGene> neuronGeneSet;

        public IEnumerable<CPPNNEATLinkGene> ValidLinks
        {
            get
            {
                return LinkGenes.Where(link => link.Enabled && !orphanedNeurons.Contains(link.To) &&
                    !orphanedNeurons.Contains(link.From));
            }
        }

        public CPPNNetwork Phenome
        {
            get;
            private set;
        }

        public CPPNNEATGeneCollection(ICPPNNEATGenome parent)
        {
            this.Parent = parent;
            this.linkGeneMap = new Dictionary<int, CPPNNEATLinkGene>();
            this.neuronGeneSet = new HashSet<CPPNNEATNeuronGene>();
            this.possibleConnections = new HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>>();
            this.orphanedNeurons = new HashSet<CPPNNEATNeuronGene>();
        }

        public void Initialise()
        {
            linkGeneMap.Clear();
            neuronGeneSet.Clear();
            possibleConnections.Clear();
            orphanedNeurons.Clear();

            sortedLinkGenesCacheExpired = true;

            foreach (var linkGene in ParentGA.DefaultLinkGenes)
            {
                TryAddLinkGene(new CPPNNEATLinkGene(linkGene.InnovationNumber, linkGene.From,
                                                linkGene.To, ParentGA.GetRandomWeight()));
            }
        }

        private void HasBeenUnOrphaned(CPPNNEATNeuronGene neuron, CPPNNEATLinkGene propogatingLink,
Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> parentFunction,
Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> childFunction)
        {
            if (neuron.Type != CPPNNeuronType.Hidden || !orphanedNeurons.Contains(neuron) ||
                    orphanedNeurons.Contains(parentFunction(propogatingLink)))
            {
                return;
            }

            orphanedNeurons.Remove(neuron);

            var enabledLinks = LinkGenes.Where(link => link.Enabled);

            foreach (var enabledLink in enabledLinks.Where(link => parentFunction(link) == neuron).ToList())
            {
                HasBeenUnOrphaned(childFunction(enabledLink), enabledLink, parentFunction, childFunction);
            }
        }

        public bool TryAddLinkGene(CPPNNEATLinkGene gene)
        {
            if (linkGeneMap.ContainsKey(gene.InnovationNumber) && linkGeneMap[gene.InnovationNumber].Enabled)
            {
                return false;
            }

            linkGeneMap[gene.InnovationNumber] = gene;

            AddNeuronGene(gene.From);
            AddNeuronGene(gene.To);

            if (gene.Enabled)
            {
                possibleConnections.Remove(Tuple.Create(gene.From, gene.To));
            }

            sortedLinkGenesCacheExpired = true;

            HasBeenUnOrphaned(gene.To, gene, link => link.From, link => link.To);
            HasBeenUnOrphaned(gene.From, gene, link => link.To, link => link.From);

            Parent.Update();

            return true;
        }

        private void AddNeuronGene(CPPNNEATNeuronGene neuron)
        {
            if (!neuronGeneSet.Contains(neuron))
            {
                neuronGeneSet.Add(neuron);

                foreach (var toNeuron in neuronGeneSet.Where(gene => gene.Type != CPPNNeuronType.Input &&
                                                gene.Type != CPPNNeuronType.Bias && (
                                                    !ParentGA.FeedForwardOnly ||
                                                    gene.Level > neuron.Level
                                                )))
                {
                    possibleConnections.Add(Tuple.Create(neuron, toNeuron));
                }

                if (neuron.Type != CPPNNeuronType.Input && neuron.Type != CPPNNeuronType.Bias)
                {
                    foreach (var fromNeuron in neuronGeneSet.Where(gene => !ParentGA.FeedForwardOnly ||
                                            gene.Level < neuron.Level
                                        ))
                    {
                        possibleConnections.Add(Tuple.Create(fromNeuron, neuron));
                    }
                }

                Parent.Update();
            }
        }

        private bool TryCreateLinkGene(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            if (to.Level == 0)
            {
                throw new ApplicationException("Links should not go into input or bias neurons");
            }
            else if (ParentGA.FeedForwardOnly && from.Level >= to.Level)
            {
                throw new ApplicationException("Cannot create recursive connections in feed forward only networks");
            }

            return TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(from, to), from, to,
                    ParentGA.GetRandomWeight()));
        }

        private void CreateNeuronGene(CPPNNEATLinkGene selectedLink)
        {
            DisableLinkGene(selectedLink);

            var newNeuron = ParentGA.GetHiddenNeuron(selectedLink.InnovationNumber);

            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(selectedLink.From, newNeuron),
                                        selectedLink.From, newNeuron, 1));
            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(newNeuron, selectedLink.To),
                                        newNeuron, selectedLink.To, selectedLink.Weight));
        }

        private void HasBeenOrphaned(CPPNNEATNeuronGene neuron, CPPNNEATLinkGene propogatingLink,
Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> parentFunction,
Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> childFunction)
        {
            if (neuron.Type != CPPNNeuronType.Hidden || orphanedNeurons.Contains(neuron))
            {
                return;
            }

            var validLinks = ValidLinks;

            if (validLinks.Where(link => link != propogatingLink && childFunction(link) == neuron).Count() == 0)
            {
                orphanedNeurons.Add(neuron);

                foreach (var validLink in validLinks.Where(link => parentFunction(link) == neuron).ToList())
                {
                    HasBeenOrphaned(childFunction(validLink), validLink, parentFunction, childFunction);
                }
            }
        }

        private void DisableLinkGene(CPPNNEATLinkGene selectedLink)
        {
            selectedLink.Enabled = false;

            possibleConnections.Add(Tuple.Create(selectedLink.From, selectedLink.To));

            HasBeenOrphaned(selectedLink.To, selectedLink, link => link.From, link => link.To);
            HasBeenOrphaned(selectedLink.From, selectedLink, link => link.To, link => link.From);

            Parent.Update();
        }

        private void EnableLinkGene(CPPNNEATLinkGene selectedLink)
        {
            selectedLink.Enabled = true;

            possibleConnections.Remove(Tuple.Create(selectedLink.From, selectedLink.To));

            HasBeenUnOrphaned(selectedLink.To, selectedLink, link => link.From, link => link.To);
            HasBeenUnOrphaned(selectedLink.From, selectedLink, link => link.To, link => link.From);

            Parent.Update();
        }

        public void DisableLinkGene(int innovationNumber)
        {
            DisableLinkGene(linkGeneMap[innovationNumber]);
        }

        public void EnableLinkGene(int innovationNumber)
        {
            EnableLinkGene(linkGeneMap[innovationNumber]);
        }

        public void Update()
        {
            Phenome = new CPPNNetwork();

            var validLinks = ValidLinks;
            var enabledNeurons = validLinks.Neurons().Union(neuronGeneSet.Where(neuron => neuron.Type == CPPNNeuronType.Input ||
                                                                                        neuron.Type == CPPNNeuronType.Output));

            foreach (var neuronGene in enabledNeurons)
            {
                neuronGene.Update();
                Phenome.AddNeuron(neuronGene.Phene);
            }

            foreach (var linkGene in validLinks)
            {
                Phenome.AddLink(linkGene.From.Phene, linkGene.To.Phene, linkGene.Weight);
            }
        }

        private CPPNNEATLinkGene GetRandomEnabledLinkGene()
        {
            return LinkGenes.Where(link => link.Enabled).ToList().RandomSingle();
        }

        public bool TryCreateNeuronGene()
        {
            var selectedLink = GetRandomEnabledLinkGene();

            if (selectedLink == null)
            {
                return false;
            }

            CreateNeuronGene(selectedLink);
            return true;
        }

        public bool TryCreateLinkGene()
        {
            var connection = possibleConnections.ToList().RandomSingle();

            if (connection == null)
            {
                return false;
            }

            return TryCreateLinkGene(connection.Item1, connection.Item2);
        }

        public bool TryDisableLinkGene()
        {
            var selectedLink = GetRandomEnabledLinkGene();

            if (selectedLink == null)
            {
                return false;
            }

            DisableLinkGene(selectedLink);
            return true;
        }
    }
}