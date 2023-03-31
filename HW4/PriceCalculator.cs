namespace HW4;
/// <summary>
/// Class to calculate price of goods.
/// </summary>
public static class PriceCalculator
{
    private const decimal WeightToPriceRatio = 1.34m;
    private const decimal VolumeToPriceRatio = 1.34m;

    /// <summary>
    /// Calculates price by good's parameters.
    /// </summary>
    /// <param name="good"></param>
    /// <returns></returns>
    public static decimal CalculatePrice(Good good)
    {
        var volumePrice = 0m;
        var weightPrice = 0m;
        for (var i = 0; i < 1000000; ++i) // imitation of time-consuming process.
        {
            volumePrice = CalculatePriceByVolume(good);
            weightPrice = CalculatePriceByWeight(good);
        }
        return Math.Max(volumePrice, weightPrice);
    }

    /// <summary>
    /// Calculates price by volume.
    /// </summary>
    /// <param name="good"></param>
    /// <returns></returns>
    private static decimal CalculatePriceByVolume(Good good)
    {
        return good.Length * good.Width * good.Height * VolumeToPriceRatio;
    }
    /// <summary>
    /// Calculates price by weight.
    /// </summary>
    /// <param name="good"></param>
    /// <returns></returns>
    private static decimal CalculatePriceByWeight(Good good)
    {

        return good.Weight * WeightToPriceRatio;
    }
}