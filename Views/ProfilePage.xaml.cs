using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        birthDatePicker.MaximumDate = DateTime.Today;
        LoadProfile();
        BindableLayout.SetItemsSource(contractsLayout, Session.Contracts.AsEnumerable().Reverse().ToList());
        contractCountLabel.Text = Session.Contracts.Count == 1
            ? "1 contrat enregistré"
            : $"{Session.Contracts.Count} contrats enregistrés";
        emptyContractsLabel.IsVisible = Session.Contracts.Count == 0;
    }

    void LoadProfile()
    {
        var user = Session.User;
        nameEntry.Text = user?.FullName ?? string.Empty;
        phoneEntry.Text = user?.Phone ?? string.Empty;
        addressEntry.Text = user?.Address ?? string.Empty;
        birthDatePicker.Date = user?.BirthDate ?? new DateTime(1990, 1, 1);

        nameLabel.Text = string.IsNullOrWhiteSpace(user?.FullName) ? "Votre nom" : user.FullName;
        phoneLabel.Text = string.IsNullOrWhiteSpace(user?.Phone) ? "Ajoutez votre téléphone" : user.Phone;
        initialsLabel.Text = string.Concat((user?.FullName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0])));
        if (string.IsNullOrEmpty(initialsLabel.Text))
            initialsLabel.Text = "?";
    }

    async void OnSaveProfile(object? sender, EventArgs e)
    {
        var name = nameEntry.Text?.Trim() ?? string.Empty;
        var phone = phoneEntry.Text?.Trim() ?? string.Empty;
        var address = addressEntry.Text?.Trim() ?? string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (name.Length < 3)
        {
            await DisplayAlertAsync("Nom incomplet", "Saisissez votre nom complet.", "OK");
            return;
        }
        if (digits.Length < 9)
        {
            await DisplayAlertAsync("Téléphone invalide", "Saisissez un numéro de téléphone valide.", "OK");
            return;
        }
        if (birthDatePicker.Date > DateTime.Today.AddYears(-18))
        {
            await DisplayAlertAsync("Âge minimum", "Vous devez avoir au moins 18 ans.", "OK");
            return;
        }
        if (address.Length < 2)
        {
            await DisplayAlertAsync("Adresse manquante", "Saisissez votre ville et votre adresse.", "OK");
            return;
        }

        Session.User = new UserInfo
        {
            FullName = name,
            Phone = phone,
            BirthDate = birthDatePicker.Date ?? new DateTime(1990, 1, 1),
            Address = address
        };
        LoadProfile();
        await DisplayAlertAsync("Profil enregistré", "Vos informations sont enregistrées sur cet appareil.", "OK");
    }

    async void OnContracts(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//main/contracts");

    async void OnClaim(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ClaimPage));

    async void OnLogout(object? sender, EventArgs e)
    {
        Session.User = null;
        LoadProfile();
        await Shell.Current.GoToAsync("//splash");
    }
}
