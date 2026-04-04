using Microsoft.EntityFrameworkCore;
using Pluralsight.CleanArchitecture.Application.Contracts;
using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Infrastructure.Persistence;

public class RecipeRepository : IRecipeRepository
{
    private readonly RecipeCatalogDbContext _context;

    public RecipeRepository(RecipeCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null)
    {
        var query = _context.Recipes.Include(r => r.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == categoryId.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(r => r.DifficultyLevel == difficulty.Value);
        }

        return await query.OrderBy(r => r.Title).ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        return await _context.Recipes.FindAsync(id);
    }

    public async Task<Recipe> AddAsync(Recipe recipe)
    {
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }
}
