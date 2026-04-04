namespace Pluralsight.CleanArchitecture.Domain.Entities;

public class Recipe
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public int DifficultyLevel { get; private set; }

    public int PrepTimeInMinutes { get; private set; }

    public void UpdatePreparation(int difficultyLevel, int prepTimeInMinutes)
    {
        if (difficultyLevel < 1 || difficultyLevel > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(difficultyLevel), "Difficulty must be between 1 and 5.");
        }

        if (prepTimeInMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prepTimeInMinutes), "Prep time must be greater than zero.");
        }

        if (prepTimeInMinutes > 60 && difficultyLevel < 3)
        {
            throw new ArgumentException("Recipes over one hour must have a difficulty of at least 3.");
        }

        DifficultyLevel = difficultyLevel;
        PrepTimeInMinutes = prepTimeInMinutes;
    }
}
