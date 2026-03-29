namespace Pluralsight.CleanArchitecture.Domain.Entities;

public class Recipe
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

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

        DifficultyLevel = difficultyLevel;
        PrepTimeInMinutes = prepTimeInMinutes;
    }
}
