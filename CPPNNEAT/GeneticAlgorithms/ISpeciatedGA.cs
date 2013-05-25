namespace CPPNNEAT.GeneticAlgorithms
{
    public interface ISpeciatedGA : IGA
    {
        double InterSpeciesMatingRate { get; set; }

        double CompatibilityDistanceThreshold { get; set; }

        int NoInnovationThreshold { get; set; }
    }
}