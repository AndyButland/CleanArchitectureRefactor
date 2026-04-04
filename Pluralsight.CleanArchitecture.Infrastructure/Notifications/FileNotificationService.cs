using Pluralsight.CleanArchitecture.Application.Contracts;

namespace Pluralsight.CleanArchitecture.Infrastructure.Notifications;

public class FileNotificationService : INotificationService
{
    public async Task SendNotificationAsync(string message)
    {
        var notification = $"[{DateTime.UtcNow:O}] {message}\n";
        await File.AppendAllTextAsync(
            Path.Combine("notifications", "recipe-notifications.txt"), notification);
    }
}
