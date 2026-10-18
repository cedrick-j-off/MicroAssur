using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class PersonalInfoPage : ContentPage
{
    public PersonalInfoPage()
    {
        InitializeComponent();
        dobPicker.MaximumDate = DateTime.Today;
        dobPicker.Date = new DateTime(1990, 1, 1);

        if (Session.User is { } u)
        {
            nameEntry.Text = u.FullName;
            phoneEntry.Text = u.Phone;
            dobPicker.Date = u.BirthDate;
            addressEntry.Text = u.Address;
        }
    }

    async void OnContinue(object? sender, EventArgs e)
    {
        var name = nameEntry.Text?.Trim() ?? "";
        var phone = phoneEntry.Text?.Trim() ?? "";
        var address = addressEntry.Text?.Trim() ?? "";
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (name.Length < 3)
        {
            await DisplayAlert("Information manquante", "Veuillez saisir votre nom complet.", "OK");
            return;
        }
        if (digits.Length < 9)
        {
            await DisplayAlert("Téléphone invalide", "Veuillez saisir un numéro de téléphone valide.", "OK");
            return;
        }
        if (dobPicker.Date > DateTime.Today.AddYears(-18))
        {
            await DisplayAlert("Âge minimum", "Vous devez avoir au moins 18 ans pour souscrire.", "OK");
            return;
        }
        if (address.Length < 2)
        {
            await DisplayAlert("Information manquante", "Veuillez saisir votre adresse.", "OK");
            return;
        }

        Session.User = new UserInfo { FullName = name, Phone = phone, BirthDate = dobPicker.Date ?? new DateTime(1990, 1, 1), Address = address };
        await Shell.Current.GoToAsync(nameof(SummaryPage));
    }
}

