using System;

namespace GeneticAlgorithms
{
    public interface IGA
    {
        bool Failed
        {
            get;
        }

        Random Random
        {
            get;
        }

        void Initialise();

        void Update();

        void UpdateGenomes();
    }
}