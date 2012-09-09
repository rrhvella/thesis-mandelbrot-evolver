using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        }

        public bool Enabled
        {
            get
            {
                return false;
            }
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

        public CPPNNetwork CreateNetwork()
        {
            throw new NotImplementedException();
        }

        public void AddLinkGene(CPPNNEATLinkGene gene)
        {
            LinkGenes.Add(gene);
        }

        public void CreateLinkGene(int neuronGeneIndex1, int neuronGeneIndex2)
        {
            LinkGenes.Add(new CPPNNEATLinkGene(NeuronGenes[neuronGeneIndex1], 
                                             NeuronGenes[neuronGeneIndex2]));
        }

        public void CreateNeuronGene(int linkGeneIndex)
        {
            var selectedLink = LinkGenes[linkGeneIndex];
            selectedLink.Enabled = false;
        }

        public void Initialise()
        {
            throw new NotImplementedException();
        }
    }
}
