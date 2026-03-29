namespace Pluralsight.CleanArchitecture.Application.Contracts;

public interface INotificationService
{
    Task SendNotificationAsync(string message);
}
