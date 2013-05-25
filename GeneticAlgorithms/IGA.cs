using System;

namespace GeneticAlgorithms
{
    public interface IGA
    {
        bool Failed
        {
            get;
        }

        void Initialise();

        void Update();

        void UpdateGenomes();
    }
}