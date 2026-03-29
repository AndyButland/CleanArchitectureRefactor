using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Application.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
}
