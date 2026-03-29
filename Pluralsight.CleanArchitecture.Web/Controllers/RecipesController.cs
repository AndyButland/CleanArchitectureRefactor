using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pluralsight.CleanArchitecture.Application.Services;
using Pluralsight.CleanArchitecture.Web.ViewModels;

namespace Pluralsight.CleanArchitecture.Web.Controllers;

public class RecipesController : Controller
{
    private readonly IRecipeService _recipeService;

    private readonly ICategoryService _categoryService;

    public RecipesController(IRecipeService recipeService, ICategoryService categoryService)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(Guid? categoryId, int? difficulty)
    {
        var recipes = await _recipeService.GetAllAsync(categoryId, difficulty);
        var categories = await _categoryService.GetAllAsync();

        var viewModel = new RecipeIndexViewModel
        {
            SelectedCategoryId = categoryId,
            SelectedDifficulty = difficulty,
            Categories = categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList(),
            Recipes = recipes.Select(r => new RecipeListItem
            {
                Id = r.Id,
                Title = r.Title,
                CategoryName = categories.FirstOrDefault(c => c.Id == r.CategoryId)?.Name ?? "",
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
        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        try
        {
            await _recipeService.CreateAsync(
                viewModel.Title, viewModel.Description, viewModel.CategoryId,
                viewModel.DifficultyLevel, viewModel.PrepTimeInMinutes);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var recipe = await _recipeService.GetByIdAsync(id);

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
        if (!ModelState.IsValid)
        {
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        try
        {
            await _recipeService.UpdateAsync(
                id, viewModel.Title, viewModel.Description, viewModel.CategoryId,
                viewModel.DifficultyLevel, viewModel.PrepTimeInMinutes);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            viewModel.Categories = await GetCategorySelectList();
            return View(viewModel);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var recipe = await _recipeService.GetByIdAsync(id);

        if (recipe == null)
        {
            return NotFound();
        }

        var categories = await _categoryService.GetAllAsync();

        var viewModel = new RecipeDeleteViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            CategoryName = categories.FirstOrDefault(c => c.Id == recipe.CategoryId)?.Name ?? "",
            DifficultyLevel = recipe.DifficultyLevel,
            PrepTimeInMinutes = recipe.PrepTimeInMinutes
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _recipeService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategorySelectList()
    {
        var categories = await _categoryService.GetAllAsync();
        return categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }
}
