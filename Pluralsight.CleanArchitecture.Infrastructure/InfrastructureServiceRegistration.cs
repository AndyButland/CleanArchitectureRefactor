using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pluralsight.CleanArchitecture.Application.Contracts;
using Pluralsight.CleanArchitecture.Infrastructure.Notifications;
using Pluralsight.CleanArchitecture.Infrastructure.Persistence;

namespace Pluralsight.CleanArchitecture.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<RecipeCatalogDbContext>(options =>
            options.UseInMemoryDatabase("RecipeCatalog"));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<INotificationService, FileNotificationService>();

        return services;
    }
}
