using System.ComponentModel.DataAnnotations;

namespace Pluralsight.CleanArchitecture.Web.Models;

public class Recipe
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    [Required]
    [Range(1, 5)]
    public int DifficultyLevel { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Prep time must be greater than zero.")]
    public int PrepTimeInMinutes { get; set; }
}
