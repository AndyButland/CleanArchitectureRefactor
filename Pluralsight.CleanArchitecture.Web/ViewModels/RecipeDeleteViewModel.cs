namespace Pluralsight.CleanArchitecture.Web.ViewModels;

public class RecipeDeleteViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int DifficultyLevel { get; set; }

    public int PrepTimeInMinutes { get; set; }
}
