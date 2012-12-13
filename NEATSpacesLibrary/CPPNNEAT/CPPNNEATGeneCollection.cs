using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEATSpacesLibrary.Extensions;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// Extends the number of methods present in IEnumerable<CPPNNEATLinkGene>.
    /// </summary>
    public static class LinkGenesListExtensions
    {
        /// <summary>
        /// Returns all of the neurons belonging to each link in this enumerable.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CPPNNEATNeuronGene> Neurons(this IEnumerable<CPPNNEATLinkGene> self) 
        {
            return self.Select(elem => elem.From).Union(self.Select(elem => elem.To))
                    .Distinct();
        }
    }

    /// <summary>
    /// The gene sequence of a CPPN-NEAT genome.
    /// </summary>
    public class CPPNNEATGeneCollection 
    {
        /// <summary>
        /// Maps a link innovation number to its corresponding link.
        /// </summary>
        private Dictionary<int, CPPNNEATLinkGene> linkGeneMap;

        /// <summary>
        /// The genome this gene-sequence belongs to.
        /// </summary>
        internal ICPPNNEATGenome Parent
        {
            get;
            set;
        }

        /// <summary>
        /// The activation functions of all the neurons belonging to this gene-sequence.
        /// </summary>
        public IEnumerable<CPPNNEATActivationFunction> ActivationFunctions
        {
            get
            {
                return neuronGeneSet.Select(neuronGene => neuronGene.ActivationFunction)
                                .Where(func => func != null);
            }
        }

        /// <summary>
        /// The genetic algorithm this gene-sequence belongs to. 
        /// </summary>
        public ICPPNNEATGA ParentGA 
        {
            get
            {
                return (ICPPNNEATGA)Parent.Parent;
            }
        }

        /// <summary>
        /// True if the sorted gene sequence needs to be rebuilt.
        /// </summary>
        private bool sortedLinkGenesCacheExpired;

        /// <summary>
        /// Sorted version of the genes in this sequence.
        /// </summary>
        private List<CPPNNEATLinkGene> sortedLinkGenes;

        /// <summary>
        /// Pairs of neurons in this sequence which can be linked.
        /// </summary>
        private HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>> possibleConnections;

        /// <summary>
        /// Neurons, belonging to this sequence, which are either disconnected from the input, or the
        /// output.
        /// </summary>
        private HashSet<CPPNNEATNeuronGene> orphanedNeurons;

        /// <summary>
        /// The link genes in this sequence sorted by innovation number.
        /// </summary>
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

                return sortedLinkGenes.AsReadOnly();
            }
        }

        /// <summary>
        /// The neurons belonging to the link genes in this sequence.
        /// </summary>
        private ISet<CPPNNEATNeuronGene> neuronGeneSet;

        /// <summary>
        /// The links which are not connected to orphaned neurons. <seealso cref="orphanedNeurons"/>
        /// </summary>
        public IEnumerable<CPPNNEATLinkGene> ValidLinks
        {
            get
            {
                return LinkGenes.Where(link => link.Enabled && !orphanedNeurons.Contains(link.To) && 
                    !orphanedNeurons.Contains(link.From));
            }
        }

        /// <summary>
        /// The phenome generated from this gene-sequence.
        /// </summary>
        public CPPNNetwork Phenome
        {
            get;
            private set;
        }

        public CPPNNEATGeneCollection()
        {
            this.linkGeneMap = new Dictionary<int, CPPNNEATLinkGene>();
            this.neuronGeneSet = new HashSet<CPPNNEATNeuronGene>();
            this.possibleConnections = new HashSet<Tuple<CPPNNEATNeuronGene, CPPNNEATNeuronGene>>();
            this.orphanedNeurons = new HashSet<CPPNNEATNeuronGene>();
        }

        /// <summary>
        /// Initialises this gene collection to the default architecture.
        /// </summary>
        public void Initialise()
        {
            sortedLinkGenesCacheExpired = true;

            foreach (var linkGene in ParentGA.DefaultLinkGenes)
            {
                TryAddLinkGene(new CPPNNEATLinkGene(linkGene.InnovationNumber, linkGene.From, 
                                                linkGene.To, ParentGA.GetRandomWeight()));
            }
        }

        /// <summary>
        /// If it can do so, removes the given neuron from the orphaned neuron list and moves along the links the neuron is 
        /// connected to, to its children.
        /// </summary>
        /// <param name="neuron"></param>
        /// <param name="propogatingLink">The link through which the algorithm travelled to arrive at this neuron.</param>
        /// <param name="parentFunction">The function which, given a link, gives the neuron the algorithm has travelled from.</param>
        /// <param name="childFunction">The function which, given a link, gives the next neuron the algorithm will travel to.</param>
        /// <remarks>
        ///Only mark this neuron as un-orphaned if the following apply:
        ///1. It is not an input/output neuron.
        ///2. It is orphaned.
        ///3. It's parent is not orphaned.
        ///
        ///If the neuron will not be un-orphaned, do not continue to its children. 
        /// </remarks>
        private void HasBeenUnOrphaned(CPPNNEATNeuronGene neuron, CPPNNEATLinkGene propogatingLink, 
                        Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> parentFunction, 
                        Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> childFunction)

        {
            if(neuron.Type != CPPNNeuronType.Hidden || !orphanedNeurons.Contains(neuron) || 
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

        /// <summary>
        /// Attempts to add the given link gene to the sequence.
        /// </summary>
        /// <param name="gene"></param>
        /// <returns>True if the link gene was succesfully added.</returns>
        public bool TryAddLinkGene(CPPNNEATLinkGene gene)
        {
            //Do not add this gene if it already exists and is enabled in this sequence.
            if (linkGeneMap.ContainsKey(gene.InnovationNumber) && linkGeneMap[gene.InnovationNumber].Enabled)
            {
                return false;
            }

            //Otherwise add the gene and register the neurons.
            linkGeneMap[gene.InnovationNumber] = gene;

            AddNeuronGene(gene.From);
            AddNeuronGene(gene.To);

            //If the given gene is enabled, remove the corresponding neuron pair from the set of possible 
            //connections.
            if (gene.Enabled)
            {
                possibleConnections.Remove(Tuple.Create(gene.From, gene.To));
            }

            //Make sure that the gene-sequence is sorted when it is accessed.
            sortedLinkGenesCacheExpired = true;

            //Check if the genes this link is connected to, as well as their children, can be un-orphaned.
            HasBeenUnOrphaned(gene.To, gene, link => link.From, link => link.To);
            HasBeenUnOrphaned(gene.From, gene, link => link.To, link => link.From);

            //Notify the parent genome that it has been updated.
            Parent.Update();

            //Return success.
            return true;
        }

        /// <summary>
        /// Register the given neuron gene as belonging to the links in this sequence.
        /// </summary>
        /// <param name="neuron"></param>
        private void AddNeuronGene(CPPNNEATNeuronGene neuron)
        {
            //Skip neurons which have already been registered.
            if (!neuronGeneSet.Contains(neuron))
            {
                //Add the neuron.
                neuronGeneSet.Add(neuron);

                //Register all the possible connections that can be made from this neuron.
                foreach (var toNeuron in neuronGeneSet.Where(gene => gene.Type != CPPNNeuronType.Input &&
                                                                gene.Type != CPPNNeuronType.Bias && (
                                                                    !ParentGA.FeedForwardOnly ||
                                                                    gene.Level > neuron.Level
                                                                )))
                {
                    possibleConnections.Add(Tuple.Create(neuron, toNeuron));
                }

                //If this neuron is not a bias or input.
                if (neuron.Type != CPPNNeuronType.Input && neuron.Type != CPPNNeuronType.Bias)
                {
                    //Register all the possible connections that can be made to this neuron.
                    foreach (var fromNeuron in neuronGeneSet.Where(gene => !ParentGA.FeedForwardOnly ||
                                                                gene.Level < neuron.Level
                                                            ))
                    {
                        possibleConnections.Add(Tuple.Create(fromNeuron, neuron));
                    }
                }

                //Notify the parent genome that it has been updated.
                Parent.Update();
            }
        }

        /// <summary>
        /// Attempts to create and add a new link gene between the given neurons.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>True if the link gene was succesfully added.</returns>
        private bool TryCreateLinkGene(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to)
        {
            //Assertions.
            if (to.Level == 0)
            {
                throw new ApplicationException("Links should not go into input or bias neurons");
            }
            else if (ParentGA.FeedForwardOnly && from.Level >= to.Level)
            {
                throw new ApplicationException("Cannot create recursive connections in feed forward only networks");
            }

            //Create and attempts to add the link.
            return TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(from, to), from, to, 
                                ParentGA.GetRandomWeight()));
        }

        /// <summary>
        /// Split the given link and place the given neuron between the new links.
        /// </summary>
        /// <param name="selectedLink"></param>
        private void CreateNeuronGene(CPPNNEATLinkGene selectedLink)
        {
            DisableLinkGene(selectedLink);

            var newNeuron = ParentGA.GetHiddenNeuron(selectedLink.InnovationNumber);

            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(selectedLink.From, newNeuron), 
                                        selectedLink.From, newNeuron, 1));
            TryAddLinkGene(new CPPNNEATLinkGene(ParentGA.GetEdgeInnovationNumber(newNeuron, selectedLink.To), 
                                        newNeuron, selectedLink.To, selectedLink.Weight));
        }

        /// <summary>
        /// If it can do so, add the given neuron to the orphaned neuron list and moves along the links the neuron is 
        /// connected to, to its children.
        /// </summary>
        /// <param name="neuron"></param>
        /// <param name="propogatingLink">The link through which the algorithm travelled to arrive at this neuron.</param>
        /// <param name="parentFunction">The function which, given a link, gives the neuron the algorithm has travelled from.</param>
        /// <param name="childFunction">The function which, given a link, gives the next neuron the algorithm will travel to.</param>
        /// <remarks>
        ///Only mark this neuron as orphaned if the following apply:
        ///1. It is not an input/output neuron.
        ///2. It is not orphaned.
        ///3. If this neuron is not the child of any other un-orphaned neuron.
        ///
        ///If the neuron will not be orphaned, do not continue to its children. 
        /// </remarks>
        private void HasBeenOrphaned(CPPNNEATNeuronGene neuron, CPPNNEATLinkGene propogatingLink, 
                        Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> parentFunction, 
                        Func<CPPNNEATLinkGene, CPPNNEATNeuronGene> childFunction)

        {
            if(neuron.Type != CPPNNeuronType.Hidden || orphanedNeurons.Contains(neuron)) 
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

        /// <summary>
        /// Mark the given link as disabled.
        /// </summary>
        /// <param name="selectedLink"></param>
        private void DisableLinkGene(CPPNNEATLinkGene selectedLink)
        {
            //Mark it as disabled.
            selectedLink.Enabled = false;
            
            //Add the pair of neurons it is connected to as a possible connection.
            possibleConnections.Add(Tuple.Create(selectedLink.From, selectedLink.To));

            //Check if the genes this link is connected to, as well as their children, can be orphaned.
            HasBeenOrphaned(selectedLink.To, selectedLink, link => link.From, link => link.To);
            HasBeenOrphaned(selectedLink.From, selectedLink, link => link.To, link => link.From);

            //Inform the parent that it has been updated.
            Parent.Update();
        }

        /// <summary>
        /// Mark the given link as enabled.
        /// </summary>
        /// <param name="selectedLink"></param>
        private void EnableLinkGene(CPPNNEATLinkGene selectedLink)
        {
            //Mark it as enabled.
            selectedLink.Enabled = true;
            
            //Add the pair of neurons it is connected to as a possible connection.
            possibleConnections.Remove(Tuple.Create(selectedLink.From, selectedLink.To));

            //Check if the genes this link is connected to, as well as their children, can be un-orphaned.
            HasBeenUnOrphaned(selectedLink.To, selectedLink, link => link.From, link => link.To);
            HasBeenUnOrphaned(selectedLink.From, selectedLink, link => link.To, link => link.From);

            //Inform the parent that it has been updated.
            Parent.Update();
        }

        /// <summary>
        /// Mark the link with the given innovation number as disabled.
        /// </summary>
        /// <param name="innovationNumber"></param>
        public void DisableLinkGene(int innovationNumber)
        {
            DisableLinkGene(linkGeneMap[innovationNumber]);
        }

        /// <summary>
        /// Mark the link with the given innovation number as enabled.
        /// </summary>
        /// <param name="innovationNumber"></param>
        public void EnableLinkGene(int innovationNumber)
        {
            EnableLinkGene(linkGeneMap[innovationNumber]);
        }

        /// <summary>
        /// Builds the phenome (CPPN network) corresponding to this gene sequence.
        /// </summary>
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

        /// <summary>
        /// Returns a random enabled link gene.
        /// </summary>
        /// <returns></returns>
        private CPPNNEATLinkGene GetRandomEnabledLinkGene()
        {
            return LinkGenes.Where(link => link.Enabled).ToList().RandomSingle(); 
        }

        /// <summary>
        /// Split a random link gene and place a neuron between the new links.
        /// </summary>
        /// <returns>True if a neuron was succesfully added.</returns>
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

        /// <summary>
        /// Try to create a new link between two random neurons.
        /// </summary>
        /// <returns>True if a link gene was succesfully added.</returns>
        /// <remarks>
        /// This method will work within the feed forward constraint if it has been set for the parent GA.
        /// </remarks>
        public bool TryCreateLinkGene()
        {
            var connection = possibleConnections.ToList().RandomSingle();

            if (connection == null)
            {
                return false;
            }

            return TryCreateLinkGene(connection.Item1, connection.Item2);
        }

        /// <summary>
        /// Try to disable a random link gene.
        /// </summary>
        /// <returns>True if a link gene was succesfully disabled.</returns>
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
