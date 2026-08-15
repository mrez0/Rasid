namespace Rasid.Core.Abstractions;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string confirmText = "Yes");
}