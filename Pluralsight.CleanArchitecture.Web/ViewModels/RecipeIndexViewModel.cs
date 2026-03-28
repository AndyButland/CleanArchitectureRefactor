using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pluralsight.CleanArchitecture.Web.ViewModels;

public class RecipeIndexViewModel
{
    public List<RecipeListItem> Recipes { get; set; } = [];

    public List<SelectListItem> Categories { get; set; } = [];

    public Guid? SelectedCategoryId { get; set; }

    public int? SelectedDifficulty { get; set; }
}

public class RecipeListItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int DifficultyLevel { get; set; }

    public int PrepTimeInMinutes { get; set; }
}
