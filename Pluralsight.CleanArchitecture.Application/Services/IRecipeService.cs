using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Application.Services;

public interface IRecipeService
{
    Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null);

    Task<Recipe?> GetByIdAsync(Guid id);

    Task<Guid> CreateAsync(string title, string? description, Guid categoryId, int difficultyLevel, int prepTimeInMinutes);

    Task UpdateAsync(Guid id, string title, string? description, Guid categoryId, int difficultyLevel, int prepTimeInMinutes);

    Task DeleteAsync(Guid id);
}
