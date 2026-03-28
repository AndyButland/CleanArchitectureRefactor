using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pluralsight.CleanArchitecture.Web.Data;
using Pluralsight.CleanArchitecture.Web.Models;
using Pluralsight.CleanArchitecture.Web.ViewModels;

namespace Pluralsight.CleanArchitecture.Web.Controllers;

public class RecipesController : Controller
{
    private readonly RecipeCatalogDbContext _context;

    public RecipesController(RecipeCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(Guid? categoryId, int? difficulty)
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

        var recipes = await query.OrderBy(r => r.Title).ToListAsync();

        var viewModel = new RecipeIndexViewModel
        {
            SelectedCategoryId = categoryId,
            SelectedDifficulty = difficulty,
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync(),
            Recipes = recipes.Select(r => new RecipeListItem
            {
                Id = r.Id,
                Title = r.Title,
                CategoryName = r.Category.Name,
                DifficultyLevel = r.DifficultyLevel,
                PrepTimeInMinutes = r.PrepTimeInMinutes
            }).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new RecipeFormViewModel
        {
            Categories = await GetCategorySelectList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecipeFormViewModel viewModel)
    {
        if (viewModel.DifficultyLevel < 1 || viewModel.DifficultyLevel > 5)
        {
            ModelState.AddModelError(nameof(viewModel.DifficultyLevel), "Difficulty must be between 1 and 5.");
        }

        if (viewModel.PrepTimeInMinutes <= 0)
        {
            ModelState.AddModelError(nameof(viewModel.PrepTimeInMinutes), "Prep time must be greater than zero.");
        }

        if (!await _context.Categories.AnyAsync(c => c.Id == viewModel.CategoryId))
        {
            ModelState.AddModelError(nameof(viewModel.CategoryId), "The selected category does not exist.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = viewModel.Title,
            Description = viewModel.Description,
            CategoryId = viewModel.CategoryId,
            DifficultyLevel = viewModel.DifficultyLevel,
            PrepTimeInMinutes = viewModel.PrepTimeInMinutes
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        var notification = $"[{DateTime.UtcNow:O}] New recipe created: {recipe.Title} (ID: {recipe.Id})\n";
        await System.IO.File.AppendAllTextAsync(
            Path.Combine("notifications", "recipe-notifications.txt"), notification);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe == null)
        {
            return NotFound();
        }

        var viewModel = new RecipeFormViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            CategoryId = recipe.CategoryId,
            DifficultyLevel = recipe.DifficultyLevel,
            PrepTimeInMinutes = recipe.PrepTimeInMinutes,
            Categories = await GetCategorySelectList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RecipeFormViewModel viewModel)
    {
        if (viewModel.DifficultyLevel < 1 || viewModel.DifficultyLevel > 5)
        {
            ModelState.AddModelError(nameof(viewModel.DifficultyLevel), "Difficulty must be between 1 and 5.");
        }

        if (viewModel.PrepTimeInMinutes <= 0)
        {
            ModelState.AddModelError(nameof(viewModel.PrepTimeInMinutes), "Prep time must be greater than zero.");
        }

        if (!await _context.Categories.AnyAsync(c => c.Id == viewModel.CategoryId))
        {
            ModelState.AddModelError(nameof(viewModel.CategoryId), "The selected category does not exist.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe == null)
        {
            return NotFound();
        }

        recipe.Title = viewModel.Title;
        recipe.Description = viewModel.Description;
        recipe.CategoryId = viewModel.CategoryId;
        recipe.DifficultyLevel = viewModel.DifficultyLevel;
        recipe.PrepTimeInMinutes = viewModel.PrepTimeInMinutes;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return NotFound();
        }

        var viewModel = new RecipeDeleteViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            CategoryName = recipe.Category.Name,
            DifficultyLevel = recipe.DifficultyLevel,
            PrepTimeInMinutes = recipe.PrepTimeInMinutes
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategorySelectList()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }
}
