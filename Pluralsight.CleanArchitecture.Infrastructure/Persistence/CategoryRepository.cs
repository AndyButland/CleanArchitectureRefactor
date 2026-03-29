using Microsoft.EntityFrameworkCore;
using Pluralsight.CleanArchitecture.Application.Contracts;
using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Infrastructure.Persistence;

public class CategoryRepository : ICategoryRepository
{
    private readonly RecipeCatalogDbContext _context;

    public CategoryRepository(RecipeCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }
}
