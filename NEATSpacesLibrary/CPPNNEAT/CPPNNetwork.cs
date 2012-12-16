using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace NEATSpacesLibrary.CPPNNEAT
{
    /// <summary>
    /// Represents a CPPN network.
    /// </summary>
    public class CPPNNetwork
    {
        /// <summary>
        /// Maps a neuron to its index within the row/column of the adjacency matrix.
        /// </summary>
        public Dictionary<CPPNNetworkNeuron, int> neuronToIndexDict;

        /// <summary>
        /// The index of the next neuron to be added.
        /// </summary>
        private int lastIndex;

        /// <summary>
        /// The indexes of the input neurons.
        /// </summary>
        private List<int> inputNeuronIndexes;

        /// <summary>
        /// The index of the output neuron.
        /// </summary>
        private int outputNeuronIndex;

        /// <summary>
        /// True if the adjacency matrix needs to be rebuilt.
        /// </summary>
        private bool adjacencyMatrixInvalidated;
        
        /// <summary>
        /// The adjacency matrix representing the graph of the network.
        /// </summary>
        private Complex[] adjacencyMatrix;

        /// <summary>
        /// The record of the last calculated activation for each neuron.
        /// </summary>
        private List<ActivationRecord> activations;

        /// <summary>
        /// Maps a matrix index to the weights of the links parallel to 
        /// to the link represented by that index.
        /// </summary>
        private Dictionary<int, List<Complex>> parallelWeightsMap;

        /// <summary>
        /// Encapsulates information about a neuron's activation.
        /// </summary>
        private class ActivationRecord
        {
            /// <summary>
            /// The current activation of the neuron.
            /// </summary>
            public Complex? Activation;

            /// <summary>
            /// The previous activation of the neuron.
            /// </summary>
            public Complex? PreviousActivation;

            /// <summary>
            /// True if the activation of the neuron is currently being calculated.
            /// </summary>
            public bool IsCalculating;

            /// <summary>
            /// The neuron attached to this record.
            /// </summary>
            public CPPNNetworkNeuron Neuron;

            /// <summary>
            /// </summary>
            /// <param name="neuron">The neuron this record is based on.</param>
            public ActivationRecord(CPPNNetworkNeuron neuron) 
            {
                Neuron = neuron;
            }
        }


        /// <summary>
        /// Represents a state during the network activation calculation.
        /// <seealso cref="GetActivation"/>
        /// </summary>
        private class NetworkActivationState
        {
            /// <summary>
            /// Index of the node for which the activation is being added to the net. 
            /// </summary>
            public int NodeIndex;

            /// <summary>
            /// Index of the node for which the activation is being calculated.
            /// </summary>
            public int ActivationNodeIndex;

            /// <summary>
            /// Current value of the net for the node for which the activation is being calculated.
            /// </summary>
            public Complex Net;

            /// <summary>
            /// </summary>
            /// <param name="activationNodeIndex">Index of the node for which the activation is being 
            /// calculated.</param>
            /// <param name="nodeIndex">Index of the node for which the activation is being added to the 
            /// net.</param>
            /// <param name="net">Current value of the net for the node for which the activation is being 
            /// calculated.</param>
            public NetworkActivationState(int activationNodeIndex, int nodeIndex, Complex net)
            {
                this.ActivationNodeIndex = activationNodeIndex;
                this.NodeIndex = nodeIndex;
                this.Net = net;
            }
        }

        public CPPNNetwork()
        {
            this.neuronToIndexDict = new Dictionary<CPPNNetworkNeuron, int>();
            this.inputNeuronIndexes = new List<int>();

            this.adjacencyMatrixInvalidated = true;

            this.activations = new List<ActivationRecord>();
        }

        /// <summary>
        /// Returns the output neuron's activation given the specified network input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Complex GetActivation(Complex[] input)
        {
            //Enforce the number inputs based on the number of input neurons.
            if (input.Length != inputNeuronIndexes.Count)
            {
                throw new ApplicationException(String.Format("There are {0} input neurons in this network," + 
                                                        " please specify an array with {0} elements",
                                                        inputNeuronIndexes.Count));
            }

            //Rebuild the adjacency matrix if it is stale. 
            if (adjacencyMatrixInvalidated)
            {
                adjacencyMatrix = BuildAdjacencyMatrix(); 
                adjacencyMatrixInvalidated = false;
            }

            //Reset the network's activation records.
            var totalNodes = neuronToIndexDict.Count;

            foreach(var i in Enumerable.Range(0, totalNodes))
            {
                var activationRecord = activations[i];

                if(activationRecord.Neuron.NeuronType == CPPNNeuronType.Bias) 
                {
                    continue;
                }

                activationRecord.Activation = null;
                activationRecord.IsCalculating = false;
            }

            //Set the activations of the input neurons according to the input.
            var inputIndex = 0;
            foreach(var i in inputNeuronIndexes) 
            {
                activations[i].Activation = input[inputIndex++];
            }

            
            //Start calculating from the output neuron.
            var outputActivationRecord = activations[outputNeuronIndex];
            outputActivationRecord.IsCalculating = true;

            var stateStack = new Stack<NetworkActivationState>();
            var currentState = new NetworkActivationState(outputNeuronIndex, 0, 0);

            //While we're calculating the activation of the output neuron.
            while (outputActivationRecord.IsCalculating)
            {
                //Get the data for the current state.
                var currentNodeIndex = currentState.ActivationNodeIndex;
                var currentActivationRecord = activations[currentNodeIndex];

                var rowFirstCell = currentNodeIndex * totalNodes;
                var calculationSuspended = false;
               
                //Mark the calculation of this neuron's activation as initiated.
                currentActivationRecord.IsCalculating = true;
                var net = currentState.Net;

                //Go through the other neurons.
                for (int i = currentState.NodeIndex; i < totalNodes; i++)
                {
                    //If the weight is 0 then assume a link doesn't exist and skip
                    //it.
                    var weight = adjacencyMatrix[rowFirstCell + i];

                    if (weight == 0)
                    {
                        continue;
                    }

                    //Otherwise calculate the activation for the child neuron.
                    var childActivationRecord = activations[i];

                    //If there is no activation specified (i.e. if this is hidden neuron we haven't
                    //tried to activate yet).
                    if (childActivationRecord.Activation == null)
                    {
                        //Calculate its activation.
                        //If we were already calculating the activation of this neuron.
                        if (childActivationRecord.IsCalculating)
                        {
                            //Use the previous activation.
                            childActivationRecord.Activation = childActivationRecord.PreviousActivation;
                        }
                        else
                        {
                            //Otherwise stop calculating the activation of the current neuron and
                            //start calculating the activation of this neuron.
                            stateStack.Push(new NetworkActivationState(currentNodeIndex, i, net));

                            currentState = new NetworkActivationState(i, 0, 0);

                            calculationSuspended = true;
                            break;
                        }
                    }

                    //Once we have the activation of the child, add its weighted activation to the 
                    //net. Also calcalte the weighted activation for the parallel links. 
                    var childActivation = (Complex)childActivationRecord.Activation;

                    var index = rowFirstCell + i;
                    net += adjacencyMatrix[index] * childActivation;

                    if (parallelWeightsMap.ContainsKey(index))
                    {
                        foreach(var parallelSignal in parallelWeightsMap[index]
                                                    .Select(parallelWeight => parallelWeight * childActivation)) 
                        {
                            net += parallelSignal;
                        }
                    }
                }

                //If the current calculation has been suspended, move to the next.
                if (calculationSuspended)
                {
                    continue;
                }

                //Mark the calculation of this network's activation as finalised.
                currentActivationRecord.Activation = currentActivationRecord.Neuron.ActivationFunction(net);
                currentActivationRecord.IsCalculating = false;

                //If this is the output neuron, then stop calculating.
                if (currentActivationRecord == outputActivationRecord)
                {
                    continue;
                }

                //Otherwise pop the next state.
                var nodeIndexRecord = stateStack.Pop();
                currentState = nodeIndexRecord;
            }

            //Update the previous activations record.
            foreach (var activation in activations)
            {
                activation.PreviousActivation = activation.Activation;
            }

            //Return the activation of the output neuron.
            return (Complex)activations[outputNeuronIndex].Activation;
        }

        /// <summary>
        /// Stores and returns the adjacency matrix representing the network's graph.
        /// </summary>
        /// <returns></returns>
        private Complex[] BuildAdjacencyMatrix()
        {
            parallelWeightsMap = new Dictionary<int, List<Complex>>();

            var indexSet = new HashSet<int>();
            var result = new Complex[neuronToIndexDict.Count * neuronToIndexDict.Count];

            foreach (var record in neuronToIndexDict)
            {
                var synapsis = record.Key.Synapsis;

                foreach(var synapse in synapsis) 
                {
                    var index = GetMatrixIndex(synapse.Neuron, record.Key);

                    if (indexSet.Contains(index))
                    {
                        if (!parallelWeightsMap.ContainsKey(index))
                        {
                            parallelWeightsMap.Add(index, new List<Complex>());
                        }

                        parallelWeightsMap[index].Add(synapse.Weight);
                    }
                    else
                    {
                        indexSet.Add(index);
                        result[index] = synapse.Weight;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a link with the given weight between the specified neurons.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        public void AddLink(CPPNNetworkNeuron from, CPPNNetworkNeuron to, Complex weight)
        {
            //Make sure the neurons exist before attempting to connect them.
            if (neuronToIndexDict.ContainsKey(from) && neuronToIndexDict.ContainsKey(to) &&
                to.NeuronType != CPPNNeuronType.Bias && to.NeuronType != CPPNNeuronType.Input)
            {
                to.AddChild(from, weight);

                if (!adjacencyMatrixInvalidated)
                {
                    adjacencyMatrix[GetMatrixIndex(from, to)] = weight;
                }
            }
        }

        /// <summary>
        /// Returns the array index in the flattened matrix representation based on the given
        /// neurons.
        /// </summary>
        /// <param name="from">The column neuron.</param>
        /// <param name="to">The row neuron.</param>
        /// <returns></returns>
        private int GetMatrixIndex(CPPNNetworkNeuron from, CPPNNetworkNeuron to)
        {
            return neuronToIndexDict[to] * neuronToIndexDict.Count + neuronToIndexDict[from];
        }

        /// <summary>
        /// Adds the given neuron to the network.
        /// </summary>
        /// <param name="neuron"></param>
        public void AddNeuron(CPPNNetworkNeuron neuron)
        {
            //Do not add the same neuron twice.
            if (neuronToIndexDict.ContainsKey(neuron))
            {
                return;
            }

            var currentIndex = lastIndex;

            //If this is a new neuron, the adjacency matrix needs to be rebuilt.
            adjacencyMatrixInvalidated = true;
            neuronToIndexDict.Add(neuron, lastIndex++);

            //Create and add the activation record for this neuron.
            var currentActivation = new ActivationRecord(neuron);
            activations.Add(currentActivation);

            switch (neuron.NeuronType)
            {
                case CPPNNeuronType.Input:
                    inputNeuronIndexes.Add(currentIndex);
                    break;

                case CPPNNeuronType.Output:
                    outputNeuronIndex = currentIndex;
                    currentActivation.PreviousActivation = 0;
                    break;

                case CPPNNeuronType.Hidden:
                    currentActivation.PreviousActivation = 0;
                    break;

                case CPPNNeuronType.Bias:
                    currentActivation.Activation = 1;
                    break;
            }
        }

        /// <summary>
        /// The number of neurons in the network.
        /// </summary>
        public int NeuronCount
        {
            get
            {
                return neuronToIndexDict.Count;
            }
        }

        /// <summary>
        /// Resets the activation of the recurrent neurons to 0.
        /// </summary>
        public void Reset()
        {
            foreach (var activation in activations)
            {
                activation.PreviousActivation = 0;
            }
        }
    }
}
