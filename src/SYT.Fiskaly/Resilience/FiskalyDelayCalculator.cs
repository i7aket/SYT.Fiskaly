using SYT.Fiskaly.Configuration;
using SYT.Fiskaly.Exceptions;

namespace SYT.Fiskaly.Resilience;

internal static class FiskalyDelayCalculator
{
    public static TimeSpan CalculateDelay(
        Exception? exception,
        int attemptNumber,
        CategoryRetryDelays delays)
    {
        double baseDelaySeconds = exception is FiskalyApiException fiskalyEx
            ? GetCategoryDelay(fiskalyEx.Category, delays)
            : delays.TransientDelaySeconds; // HTTP errors (503, network failures) are transient by nature

        double exponentialDelay = baseDelaySeconds * Math.Pow(2, attemptNumber);

        double jitter = Random.Shared.NextDouble() * 0.5 - 0.25;
        double finalDelay = exponentialDelay * (1 + jitter);

        return TimeSpan.FromSeconds(finalDelay);
    }

    private static double GetCategoryDelay(FiskalyErrorCategory category, CategoryRetryDelays delays) =>
        category switch
        {
            FiskalyErrorCategory.Transient => delays.TransientDelaySeconds,
            FiskalyErrorCategory.Infrastructure => delays.InfrastructureDelaySeconds,
            FiskalyErrorCategory.Authentication => delays.AuthenticationDelaySeconds,

            _ => delays.TransientDelaySeconds
        };
}
