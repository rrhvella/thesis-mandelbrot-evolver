using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.CPPNNEAT
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

        internal CPPNNEATGenome Parent
        {
            get;
            set;
        }

        public CPPNNEATGA ParentGA 
        {
            get
            {
                return (CPPNNEATGA)Parent.Parent;
            }
        }

        private bool sortedLinkGenesCacheExpired;
        private List<CPPNNEATLinkGene> sortedLinkGenes;

        private HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>> possibleConnections;

        public IList<CPPNNEATLinkGene> LinkGenes
        {
            get
            {
                if (sortedLinkGenesCacheExpired)
                {
                    sortedLinkGenes = linkGeneMap.Values.ToList();
                    sortedLinkGenes.Sort(new Comparison<CPPNNEATLinkGene>(
                                delegate(CPPNNEATLinkGene first, CPPNNEATLinkGene second) {
                                    return first.InnovationNumber.CompareTo(second.InnovationNumber);
                                }
                    ));

                    sortedLinkGenesCacheExpired = false;
                }

                return sortedLinkGenes;
            }
        }

        private ISet<CPPNNEATNeuronGene> neuronGeneSet;
        public IList<CPPNNEATNeuronGene> NeuronGenes
        {
            get;
            private set;
        }

        public CPPNNetwork Phenome
        {
            get;
            private set;
        }

        public CPPNNEATGeneCollection()
        {
            this.linkGeneMap = new Dictionary<int, CPPNNEATLinkGene>();
            this.NeuronGenes = new List<CPPNNEATNeuronGene>();
            this.neuronGeneSet = new HashSet<CPPNNEATNeuronGene>();
            this.possibleConnections = new HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>>();
        }

        public void Initialise()
        {
            sortedLinkGenesCacheExpired = true;

            foreach (var linkGene in ParentGA.DefaultLinkGenes)
            {
                TryAddLinkGene(new CPPNNEATLinkGene(linkGene.InnovationNumber, linkGene.From, 
                                                linkGene.To, ParentGA.GetRandomWeight()));
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

            Parent.Update();

            return true;
        }

        private void AddNeuronGene(CPPNNEATNeuronGene neuron)
        {
            if (!neuronGeneSet.Contains(neuron))
            {
                NeuronGenes.Add(neuron);
                neuronGeneSet.Add(neuron);

                foreach (var toNeuron in NeuronGenes.Where(gene => gene.Type != CPPNNeuronType.Input &&
                                                                gene.Type != CPPNNeuronType.Bias))
                {
                    possibleConnections.Add(Tuple.Create(neuron, toNeuron));
                }

                if (neuron.Type != CPPNNeuronType.Input && neuron.Type != CPPNNeuronType.Bias)
                {
                    foreach (var fromNeuron in NeuronGenes)
                    {
                        possibleConnections.Add(Tuple.Create(fromNeuron, neuron));
                    }
                }

                Parent.Update();
            }
        }

        public bool TryCreateLinkGene(int neuronGeneIndexFrom, int neuronGeneIndexTo)
        {
            return TryCreateLinkGene(NeuronGenes[neuronGeneIndexFrom], NeuronGenes[neuronGeneIndexTo]);
        }

        private bool TryCreateLinkGene(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            if (to.Type == CPPNNeuronType.Bias || to.Type == CPPNNeuronType.Input)
            {
                throw new ApplicationException("Links should not go into input or bias neurons");
            }

            return TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetInnovationNumber(from, to), from, to, 
                                ParentGA.GetRandomWeight()));
        }

        private void CreateNeuronGene(CPPNNEATLinkGene selectedLink)
        {
            DisableLinkGene(selectedLink);

            var newNeuron = ParentGA.GetHiddenNeuron(selectedLink.InnovationNumber);

            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetInnovationNumber(selectedLink.From, newNeuron), selectedLink.From, 
                                        newNeuron, 1));
            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetInnovationNumber(newNeuron, selectedLink.To), newNeuron, selectedLink.To, 
                                        selectedLink.Weight));
        }

        public void CreateNeuronGene(int linkGeneIndex)
        {
            CreateNeuronGene(LinkGenes[linkGeneIndex]);
        }

        public void UpdateLinkGeneWeight(int linkGeneIndex, double newWeight)
        {
            LinkGenes[linkGeneIndex].Weight = newWeight;
            Parent.Update();
        }

        private void DisableLinkGene(CPPNNEATLinkGene selectedLink)
        {
            selectedLink.Enabled = false;
            possibleConnections.Add(Tuple.Create(selectedLink.From, selectedLink.To));

            Parent.Update();
        }

        public void DisableLinkGene(int innovationNumber)
        {
            DisableLinkGene(linkGeneMap[innovationNumber]);
        }

        public void Update()
        {
            Phenome = new CPPNNetwork();

            var enabledLinks = LinkGenes.Where(link => link.Enabled);
            var enabledNeurons = enabledLinks.Neurons().Union(NeuronGenes.Where(neuron => neuron.Type == CPPNNeuronType.Input ||
                                                                                        neuron.Type == CPPNNeuronType.Output));

            foreach (var neuronGene in enabledNeurons)
            {
                neuronGene.Update();
                Phenome.AddNeuron(neuronGene.Phene);
            }

            foreach (var linkGene in enabledLinks)
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
