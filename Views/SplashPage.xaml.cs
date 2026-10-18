using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage() => InitializeComponent();

    async void OnStart(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//main/catalog");

    async void OnLogin(object? sender, EventArgs e)
    {
        Session.User ??= UserInfo.Demo();
        await Shell.Current.GoToAsync("//main/contracts");
    }
}
