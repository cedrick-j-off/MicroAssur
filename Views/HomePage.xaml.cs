using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class HomePage : ContentPage
{
    public HomePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var first = Session.User?.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        greeting.Text = string.IsNullOrEmpty(first) ? "Bienvenue sur MicroAssur" : $"Bonjour {first}";
    }

    async void OnSubscribe(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//main/catalog");
    async void OnContracts(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//main/contracts");
    async void OnClaim(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync(nameof(ClaimPage));
}
