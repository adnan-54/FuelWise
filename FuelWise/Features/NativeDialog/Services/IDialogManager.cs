namespace FuelWise.NativeDialog;

public interface IDialogManager
{
    Task ShowDialog(string title, string message, string action);

    Task ShowError(string message);

    Task ShowSuccess(string message);
}

internal class DefaultDialogManager : IDialogManager
{
    public Task ShowDialog(string title, string message, string action)
    {
        if (Application.Current?.MainPage is Page page)
            return page.DisplayAlert(title, message, action);

        return Task.CompletedTask;
    }

    public Task ShowError(string message)
    {
        return ShowDialog("Erro", message, "Ok");
    }

    public Task ShowSuccess(string message)
    {
        return ShowDialog("Sucesso", message, "Ok");
    }
}
