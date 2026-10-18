using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class PaymentPage : ContentPage
{
    public PaymentPage()
    {
        InitializeComponent();
        amountLabel.Text = Fmt.Fcfa(Session.PayAmount);
    }

    void OnMtnTapped(object? sender, TappedEventArgs e) => rbMtn.IsChecked = true;
    void OnOrangeTapped(object? sender, TappedEventArgs e) => rbOrange.IsChecked = true;

    async void OnPay(object? sender, EventArgs e)
    {
        if (Session.PayAmount <= 0)
        {
            await DisplayAlert("Paiement", "Aucune échéance à payer.", "OK");
            return;
        }

        var op = rbMtn.IsChecked ? "MTN Mobile Money" : "Orange Money";
        payButton.IsEnabled = false;
        spinner.IsVisible = spinner.IsRunning = true;

        try
        {
            await Task.Delay(1500); // simulation de la validation Mobile Money

            if (Session.PayMode == PayMode.Subscription)
            {
                Session.CreateContract();
                // On vide le parcours de souscription puis on affiche l'attestation depuis l'onglet « Contrats »
                await Shell.Current.GoToAsync("//main/catalog");
                await Shell.Current.GoToAsync("//main/contracts");
                await Shell.Current.GoToAsync(nameof(CertificatePage));
            }
            else
            {
                var amount = Session.PayAmount;
                Session.PayNextInstallment();
                await DisplayAlert("Paiement réussi", $"Votre paiement de {Fmt.Fcfa(amount)} via {op} a bien été reçu.", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        finally
        {
            payButton.IsEnabled = true;
            spinner.IsVisible = spinner.IsRunning = false;
        }
    }
}
