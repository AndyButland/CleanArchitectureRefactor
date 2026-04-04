using Microsoft.EntityFrameworkCore;
using Pluralsight.CleanArchitecture.Domain.Entities;

namespace Pluralsight.CleanArchitecture.Infrastructure.Persistence;

public class RecipeCatalogDbContext : DbContext
{
    public RecipeCatalogDbContext(DbContextOptions<RecipeCatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Title).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.HasOne(r => r.Category)
                  .WithMany()
                  .HasForeignKey(r => r.CategoryId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var breakfastId = Guid.Parse("b1c2d3e4-1111-4000-a000-000000000001");
        var mainCourseId = Guid.Parse("b1c2d3e4-2222-4000-a000-000000000002");
        var dessertId = Guid.Parse("b1c2d3e4-3333-4000-a000-000000000003");
        var appetizerId = Guid.Parse("b1c2d3e4-4444-4000-a000-000000000004");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = breakfastId, Name = "Breakfast" },
            new Category { Id = mainCourseId, Name = "Main Course" },
            new Category { Id = dessertId, Name = "Dessert" },
            new Category { Id = appetizerId, Name = "Appetizer" }
        );

        var pancakes = new Recipe
        {
            Id = Guid.Parse("a0000001-0001-4000-a000-000000000001"),
            Title = "Classic Pancakes",
            Description = "Fluffy buttermilk pancakes served with maple syrup and fresh berries.",
            CategoryId = breakfastId
        };
        pancakes.UpdatePreparation(1, 20);

        var eggsBenedict = new Recipe
        {
            Id = Guid.Parse("a0000001-0002-4000-a000-000000000002"),
            Title = "Eggs Benedict",
            Description = "Poached eggs on toasted muffins with hollandaise sauce and smoked salmon.",
            CategoryId = breakfastId
        };
        eggsBenedict.UpdatePreparation(3, 35);

        var grilledChicken = new Recipe
        {
            Id = Guid.Parse("a0000001-0003-4000-a000-000000000003"),
            Title = "Grilled Chicken",
            Description = "Herb-marinated chicken breast grilled and served with roasted vegetables.",
            CategoryId = mainCourseId
        };
        grilledChicken.UpdatePreparation(2, 45);

        var beefStew = new Recipe
        {
            Id = Guid.Parse("a0000001-0004-4000-a000-000000000004"),
            Title = "Beef Stew",
            Description = "Slow-cooked beef with root vegetables in a rich red wine sauce.",
            CategoryId = mainCourseId
        };
        beefStew.UpdatePreparation(3, 120);

        var mushroomRisotto = new Recipe
        {
            Id = Guid.Parse("a0000001-0005-4000-a000-000000000005"),
            Title = "Mushroom Risotto",
            Description = "Creamy arborio rice with mixed wild mushrooms and parmesan.",
            CategoryId = mainCourseId
        };
        mushroomRisotto.UpdatePreparation(4, 50);

        var chocolateLavaCake = new Recipe
        {
            Id = Guid.Parse("a0000001-0006-4000-a000-000000000006"),
            Title = "Chocolate Lava Cake",
            Description = "Individual warm chocolate cakes with a molten center, served with vanilla ice cream.",
            CategoryId = dessertId
        };
        chocolateLavaCake.UpdatePreparation(4, 30);

        var lemonTart = new Recipe
        {
            Id = Guid.Parse("a0000001-0007-4000-a000-000000000007"),
            Title = "Lemon Tart",
            Description = "Crisp shortcrust pastry filled with tangy lemon curd and topped with meringue.",
            CategoryId = dessertId
        };
        lemonTart.UpdatePreparation(3, 60);

        var bruschetta = new Recipe
        {
            Id = Guid.Parse("a0000001-0008-4000-a000-000000000008"),
            Title = "Bruschetta",
            Description = "Toasted ciabatta topped with fresh tomatoes, basil, garlic, and olive oil.",
            CategoryId = appetizerId
        };
        bruschetta.UpdatePreparation(1, 15);

        modelBuilder.Entity<Recipe>().HasData(
            pancakes, eggsBenedict, grilledChicken, beefStew,
            mushroomRisotto, chocolateLavaCake, lemonTart, bruschetta
        );
    }
}
