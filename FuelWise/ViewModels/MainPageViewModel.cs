using CommunityToolkit.Mvvm.ComponentModel;

namespace FuelWise.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public MainPageViewModel(MainViewViewModel mainViewModel, DataViewModel dataViewModel, ConnectionViewModel connectionViewModel)
    {
        MainViewModel = mainViewModel;
        DataViewModel = dataViewModel;
        ConnectionViewModel = connectionViewModel;
    }

    public MainViewViewModel MainViewModel { get; }

    public DataViewModel DataViewModel { get; }

    public ConnectionViewModel ConnectionViewModel { get; }
}