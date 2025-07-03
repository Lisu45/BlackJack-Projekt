using BlackJack.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace BlackJack.ViewModels;
public partial class StartViewModel : ViewModelBase
{
    public event Action<string>? OnStartGame;

    [ObservableProperty]
    string username = string.Empty;

    [ObservableProperty]
    bool isUsernameValid;


    

    partial void OnUsernameChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsUsernameValid = false;
        }
        else
        {
            IsUsernameValid = true;
        }
    }

    [RelayCommand]
    public void StartGame()
    {
        OnStartGame?.Invoke(Username);
    }
}