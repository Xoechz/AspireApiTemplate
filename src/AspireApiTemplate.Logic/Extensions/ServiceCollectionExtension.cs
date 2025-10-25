using AspireApiTemplate.Logic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApiTemplate.Logic.Extensions;

public static class ServiceCollectionExtension
{
    #region Public Methods

    public static IServiceCollection AddLogicServices(this IServiceCollection services)
    {
        services.AddScoped<ExampleService>();

        return services;
    }

    #endregion Public Methods
}