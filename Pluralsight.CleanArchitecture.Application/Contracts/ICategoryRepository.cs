using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Application.Contracts;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(Guid id);
}
