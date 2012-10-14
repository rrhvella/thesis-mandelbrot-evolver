using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.CPPNNEAT
{
    public class CPPNNetwork
    {
        public Dictionary<CPPNNetworkNeuron, int> neuronToIndexDict;
        private int lastIndex;

        private List<int> inputNeuronIndexes;
        private int outputNeuronIndex;

        private bool adjacencyMatrixInvalidated;
        private double[] adjacencyMatrix;

        private List<ActivationRecord> activations;

        private class ActivationRecord
        {
            public double? Activation;
            public double? PreviousActivation;
            public bool IsCalculating;
            public CPPNNetworkNeuron Neuron;

            public ActivationRecord(CPPNNetworkNeuron neuron) 
            {
                Neuron = neuron;
            }
        }

        private class NodeIndexRecord
        {
            public int I;
            public int NodeIndex;
            public double Net;

            public NodeIndexRecord(int currentNodeIndex, int i, double net)
            {
                this.NodeIndex = currentNodeIndex;
                this.I = i;
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

        public double GetActivation(double[] input)
        {
            if (input.Length != inputNeuronIndexes.Count)
            {
                throw new ApplicationException(String.Format("There are {0} input neurons in this network," + 
                                                        " please specify an array with {0} elements",
                                                        inputNeuronIndexes.Count));
            }

            if (adjacencyMatrixInvalidated)
            {
                adjacencyMatrix = BuildAdjacencyMatrix(); 
                adjacencyMatrixInvalidated = false;
            }

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

            var inputIndex = 0;
            foreach(var i in inputNeuronIndexes) 
            {
                activations[i].Activation = input[inputIndex++];
            }

            var outputActivationRecord = activations[outputNeuronIndex];
            outputActivationRecord.IsCalculating = true;

            var nodeIndexStack = new Stack<NodeIndexRecord>();
            var currentNodeIndexRecord = new NodeIndexRecord(outputNeuronIndex, 0, 0);

            while (outputActivationRecord.IsCalculating)
            {
                var currentNodeIndex = currentNodeIndexRecord.NodeIndex;
                var currentActivationRecord = activations[currentNodeIndex];

                var rowFirstCell = currentNodeIndex * totalNodes;
                var skipWhileLoop = false;

                currentActivationRecord.IsCalculating = true;
                var net = currentNodeIndexRecord.Net;

                for (int i = currentNodeIndexRecord.I; i < totalNodes; i++)
                {
                    var childActivationRecord = activations[i];

                    if (childActivationRecord.Activation == null)
                    {
                        if (childActivationRecord.IsCalculating)
                        {
                            childActivationRecord.Activation = childActivationRecord.PreviousActivation;
                        }
                        else
                        {
                            nodeIndexStack.Push(new NodeIndexRecord(currentNodeIndex, i, net));

                            currentNodeIndexRecord = new NodeIndexRecord(i, 0, 0);

                            skipWhileLoop = true;
                            break;
                        }
                    }

                    net += adjacencyMatrix[rowFirstCell + i] * (double)childActivationRecord.Activation;
                }

                if (skipWhileLoop)
                {
                    continue;
                }

                currentActivationRecord.Activation = currentActivationRecord.Neuron.ActivationFunction(net);
                currentActivationRecord.IsCalculating = false;

                if (currentActivationRecord == outputActivationRecord)
                {
                    continue;
                }

                var nodeIndexRecord = nodeIndexStack.Pop();
                currentNodeIndexRecord = nodeIndexRecord;
            }

            foreach (var activation in activations)
            {
                activation.PreviousActivation = activation.Activation;
            }

            return (double)activations[outputNeuronIndex].Activation;
        }

        private double[] BuildAdjacencyMatrix()
        {
            var result = new double[neuronToIndexDict.Count * neuronToIndexDict.Count];

            foreach (var record in neuronToIndexDict)
            {
                var synapsis = record.Key.Synapsis;

                foreach(var synapse in synapsis) 
                {
                    result[GetMatrixIndex(synapse.Neuron, record.Key)] = synapse.Weight;
                }
            }

            return result;
        }

        public void AddLink(CPPNNetworkNeuron from, CPPNNetworkNeuron to, double weight)
        {
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

        private int GetMatrixIndex(CPPNNetworkNeuron from, CPPNNetworkNeuron to)
        {
            return neuronToIndexDict[to] * neuronToIndexDict.Count + neuronToIndexDict[from];
        }

        public void AddNeuron(CPPNNetworkNeuron neuron)
        {
            if (neuronToIndexDict.ContainsKey(neuron))
            {
                return;
            }

            var currentIndex = lastIndex;

            adjacencyMatrixInvalidated = true;
            neuronToIndexDict.Add(neuron, lastIndex++);

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

        public int NeuronCount
        {
            get
            {
                return neuronToIndexDict.Count;
            }
        }
    }
}
