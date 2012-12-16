using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using NEATSpacesLibrary.GeneticAlgorithms;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// This interface allows variables to have a CPPNNEATGA type without having to specify
    /// the type parameters.
    /// </summary>
    public interface ICPPNNEATGA : ISpeciatedGA
    {
        /// <summary>
        /// The links in the initial architecture of all individuals.
        /// </summary>
        IList<CPPNNEATLinkGene> DefaultLinkGenes { get; }

        /// <summary>
        /// If true, forces the algorithm to only generate feed forward networks.
        /// </summary>
        bool FeedForwardOnly { get;}

        /// <summary>
        /// Returns the innovation number for the link between the given neurons.
        /// </summary>
        int GetEdgeInnovationNumber(CPPNNEATNeuronGene from, CPPNNEATNeuronGene to);

        /// <summary>
        /// Returns a random weight.
        /// </summary>
        Complex GetRandomWeight();

        /// <summary>
        /// Returns the hidden neuron corresponding to the given innovation number.
        /// </summary>
        CPPNNEATNeuronGene GetHiddenNeuron(int linkIndex);

        /// <summary>
        /// Determines the rate at which weights are modified during mutation.
        /// </summary>
        double WeightMutationRate { get; set; }

        /// <summary>
        /// The maximum magnitude of the value by which a weight is modified during perturbation.
        /// </summary>
        double MaxPerturbation { get; set; }

        /// <summary>
        /// Determines the rate at which mutated weights are perturbed as opposed to reinitialised.
        /// </summary>
        double WeightPertubationRate { get; set; }

        /// <summary>
        /// Determines the rate at which new neurons are added during mutation.
        /// </summary>
        double NewNeuronRate { get; set; }

        /// <summary>
        /// Determines the rate at which new links are added during mutation.
        /// </summary>
        double NewLinkRate { get; set; }

        /// <summary>
        /// The rate at which parent weights are averaged, as opposed to selecting one or the other.
        /// </summary>
        double MateByAveragingRate { get; set; }

        /// <summary>
        /// The weight of the average difference in link weights on the compatibility distance. 
        /// </summary>
        double MatchingGenesWeight { get; set; }

        /// <summary>
        /// The weight of the number of disjoint genes on the compatibility distance. 
        /// </summary>
        double DisjointGenesWeight { get; set; }

        /// <summary>
        /// The weight of the number of excess genes on the compatibility distance. 
        /// </summary>
        double ExcessGenesWeight { get; set; }

        /// <summary>
        /// For a child genome, this determines the probability of a link gene being disabled if it is 
        /// disabled for a parent genome.
        /// </summary>
        double DisableGeneRate { get; set; }

        /// <summary>
        /// The weight of the average difference in function counts on the compatibility distance. 
        /// </summary>
        double FunctionDifferenceWeight { get; set; }
    }
}
