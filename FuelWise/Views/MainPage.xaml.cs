﻿using FuelWise.ViewModels;

namespace FuelWise;

public partial class MainPage : TabbedPage
{
    public MainPage(MainPageViewModel mainPageViewModel)
    {
        InitializeComponent();
        BindingContext = mainPageViewModel;
    }
}
