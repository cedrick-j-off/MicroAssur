using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var u = Session.User;
        nameLabel.Text = u?.FullName is { Length: > 0 } n ? n : "Invité";
        phoneLabel.Text = u?.Phone is { Length: > 0 } p ? p : "Connectez-vous pour retrouver vos informations";
        initialsLabel.Text = string.Concat(nameLabel.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                         .Take(2).Select(w => char.ToUpperInvariant(w[0])));
    }

    async void OnContracts(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync("//main/contracts");
    async void OnClaim(object? sender, TappedEventArgs e) => await Shell.Current.GoToAsync(nameof(ClaimPage));

    async void OnLogout(object? sender, EventArgs e)
    {
        Session.User = null;
        await Shell.Current.GoToAsync("//splash");
    }
}
