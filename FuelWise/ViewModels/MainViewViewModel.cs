using CommunityToolkit.Mvvm.ComponentModel;

namespace FuelWise.ViewModels;

public partial class MainViewViewModel : ObservableObject
{
    [ObservableProperty]
    private string test = "cacetola";

}
