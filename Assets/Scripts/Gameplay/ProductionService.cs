using System;

public class ProductionService
{
    private readonly FactoryService factory;

    public ProductionService(FactoryService factory)
    {
        this.factory = factory
                       ?? throw new ArgumentNullException(nameof(factory));
    }

    public double CalculateIncome(
        double elapsedSeconds,
        double multiplier = 1.0)
    {
        if (elapsedSeconds <= 0 ||
            double.IsNaN(elapsedSeconds) ||
            double.IsInfinity(elapsedSeconds))
        {
            return 0;
        }

        if (multiplier <= 0 ||
            double.IsNaN(multiplier) ||
            double.IsInfinity(multiplier))
        {
            return 0;
        }

        double income =
            factory.GetTotalProduction() * elapsedSeconds * multiplier;

        if (double.IsNaN(income) || double.IsInfinity(income))
            return 0;

        return income;
    }

    public double ApplyIncome(
        double elapsedSeconds,
        double multiplier = 1.0)
    {
        double income = CalculateIncome(elapsedSeconds, multiplier);
        factory.AddCurrency(income);
        return income;
    }
}
