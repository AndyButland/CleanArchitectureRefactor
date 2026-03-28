using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pluralsight.CleanArchitecture.Web.ViewModels;

public class RecipeFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public Guid CategoryId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Difficulty must be between 1 and 5.")]
    [Display(Name = "Difficulty Level")]
    public int DifficultyLevel { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Prep time must be greater than zero.")]
    [Display(Name = "Prep Time (minutes)")]
    public int PrepTimeInMinutes { get; set; }

    public List<SelectListItem> Categories { get; set; } = [];
}
