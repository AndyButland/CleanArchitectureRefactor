using Pluralsight.CleanArchitecture.Application.Contracts;
using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly INotificationService _notificationService;

    public RecipeService(
        IRecipeRepository recipeRepository,
        ICategoryRepository categoryRepository,
        INotificationService notificationService)
    {
        _recipeRepository = recipeRepository;
        _categoryRepository = categoryRepository;
        _notificationService = notificationService;
    }

    public async Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null)
    {
        return await _recipeRepository.GetAllAsync(categoryId, difficulty);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        return await _recipeRepository.GetByIdAsync(id);
    }

    public async Task<Guid> CreateAsync(string title, string? description, Guid categoryId,
        int difficultyLevel, int prepTimeInMinutes)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new ArgumentException("The selected category does not exist.");
        }

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CategoryId = categoryId
        };

        recipe.UpdatePreparation(difficultyLevel, prepTimeInMinutes);

        await _recipeRepository.AddAsync(recipe);

        await _notificationService.SendNotificationAsync(
            $"New recipe created: {recipe.Title} (ID: {recipe.Id})");

        return recipe.Id;
    }

    public async Task UpdateAsync(Guid id, string title, string? description, Guid categoryId,
        int difficultyLevel, int prepTimeInMinutes)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null)
        {
            throw new ArgumentException("Recipe not found.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new ArgumentException("The selected category does not exist.");
        }

        recipe.Title = title;
        recipe.Description = description;
        recipe.CategoryId = categoryId;
        recipe.UpdatePreparation(difficultyLevel, prepTimeInMinutes);

        await _recipeRepository.UpdateAsync(recipe);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _recipeRepository.DeleteAsync(id);
    }
}
