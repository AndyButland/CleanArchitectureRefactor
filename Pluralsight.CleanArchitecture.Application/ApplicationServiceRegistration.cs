using Microsoft.Extensions.DependencyInjection;
using Pluralsight.CleanArchitecture.Application.Services;

namespace Pluralsight.CleanArchitecture.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }
}
