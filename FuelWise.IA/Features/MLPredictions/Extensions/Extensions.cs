using Microsoft.Extensions.DependencyInjection;

namespace FuelWise.IA;

public static class Extensions
{
    public static IServiceCollection AddMachineLearning(this IServiceCollection services)
    {
        services.AddSingleton<IMLPredictions, MLPredictions>();

        return services;
    }
}