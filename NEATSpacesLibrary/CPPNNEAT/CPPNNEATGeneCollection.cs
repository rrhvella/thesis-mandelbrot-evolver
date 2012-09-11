using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNEATGeneCollection
    {
        public CPPNNEATGA Parent 
        { 
            get; 
            set; 
        }

        public CPPNNEATGeneCollection()
        {
            this.LinkGenes = new List<CPPNNEATLinkGene>();
            this.NeuronGenes = new List<CPPNNEATNeuronGene>();
        }

        public bool Enabled
        {
            get;
            set;
        }

        public IList<CPPNNEATLinkGene> LinkGenes
        {
            get;
            private set;
        }

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

        public void Update()
        {
            Phenome = new CPPNNetwork();

            var enabledLinks = LinkGenes.Where(link => link.Enabled);
            var enabledNeurons = enabledLinks.Select(link => link.From)
                                    .Union(enabledLinks.Select(link => link.To))
                                    .Distinct();

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

        public void AddLinkGene(CPPNNEATLinkGene gene)
        {
            LinkGenes.Add(gene);
        }

        public void AddNeuronGene(CPPNNEATNeuronGene neuron)
        {
            NeuronGenes.Add(neuron);
        }

        public void CreateLinkGene(int neuronGeneIndexFrom, int neuronGeneIndexTo)
        {
            if (NeuronGenes[neuronGeneIndexTo].Type == CPPNNeuronType.Bias ||
                    NeuronGenes[neuronGeneIndexTo].Type == CPPNNeuronType.Input)
            {
                throw new ApplicationException("Links should not go into input or bias neurons");
            }

            LinkGenes.Add(new CPPNNEATLinkGene(Parent.NextInnovationNumber(), NeuronGenes[neuronGeneIndexFrom], 
                                             NeuronGenes[neuronGeneIndexTo], Parent.GetRandomWeight()));
        }

        public void CreateNeuronGene(int linkGeneIndex)
        {
            var selectedLink = LinkGenes[linkGeneIndex];
            selectedLink.Enabled = false;

            var newNeuron = new CPPNNEATNeuronGene(CPPNNeuronType.Hidden, Parent.CanonicalFunctionList.RandomSingle());

            AddLinkGene(new CPPNNEATLinkGene(Parent.NextInnovationNumber(), selectedLink.From, newNeuron, Parent.GetRandomWeight()));
            AddLinkGene(new CPPNNEATLinkGene(Parent.NextInnovationNumber(), newNeuron, selectedLink.To, Parent.GetRandomWeight()));

            AddNeuronGene(newNeuron);
        }

        public void Initialise()
        {
            foreach (var neuronGene in Parent.DefaultNeuronGenes)
            {
                AddNeuronGene(neuronGene);
            }

            foreach (var linkGene in Parent.DefaultLinkGenes)
            {
                AddLinkGene(new CPPNNEATLinkGene(linkGene.InnovationNumber, linkGene.From, linkGene.To, Parent.GetRandomWeight()));
            }
        }
    }
}
