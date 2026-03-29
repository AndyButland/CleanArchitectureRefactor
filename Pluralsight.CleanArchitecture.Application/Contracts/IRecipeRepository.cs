using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Application.Contracts;

public interface IRecipeRepository
{
    Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null);

    Task<Recipe?> GetByIdAsync(Guid id);

    Task<Recipe> AddAsync(Recipe recipe);

    Task UpdateAsync(Recipe recipe);

    Task DeleteAsync(Guid id);
}
