using MicroAssur.Models;
using MicroAssur.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;

namespace MicroAssur.Views;

public partial class PaymentPage : ContentPage
{
    Guid? pendingPaymentId;

    public PaymentPage()
    {
        InitializeComponent();
        amountLabel.Text = Fmt.Fcfa(Session.PayAmount);
        phoneEntry.Text = Session.User?.Phone ?? "";
        simulationPicker.SelectedIndex = 0;
    }

    void OnMtnTapped(object? sender, TappedEventArgs e) => rbMtn.IsChecked = true;
    void OnOrangeTapped(object? sender, TappedEventArgs e) => rbOrange.IsChecked = true;

    async void OnPay(object? sender, EventArgs e)
    {
        if (Session.PayAmount <= 0)
        {
            await DisplayAlertAsync("Paiement", "Aucune échéance à payer.", "OK");
            return;
        }

        var phone = phoneEntry.Text?.Trim() ?? "";
        if (new string(phone.Where(char.IsDigit).ToArray()).Length < 9)
        {
            await DisplayAlertAsync("Téléphone invalide", "Veuillez saisir un numéro Mobile Money valide.", "OK");
            return;
        }

        payButton.IsEnabled = false;
        spinner.IsVisible = spinner.IsRunning = true;

        try
        {
            var user = Session.User ?? UserInfo.Demo();
            var subscription = Session.PayMode == PayMode.Subscription;
            var client = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<PaymentApiClient>();
            var payment = await client.CreatePaymentAsync(new CreatePaymentApiRequest
            {
                Provider = rbMtn.IsChecked ? "Mtn" : "Orange",
                Purpose = subscription ? "Subscription" : "Installment",
                Amount = Session.PayAmount,
                PhoneNumber = phone,
                CustomerName = user.FullName,
                BirthDate = subscription ? DateOnly.FromDateTime(user.BirthDate) : null,
                Address = subscription ? user.Address : null,
                CategoryId = subscription ? Session.CurrentCategory.Id : null,
                CategoryName = subscription ? Session.CurrentCategory.Name : null,
                PlanName = subscription ? Session.CurrentPlan.Name : null
            }, Guid.NewGuid().ToString("N"));

            if (simulationPicker.SelectedIndex < 2)
            {
                var outcome = simulationPicker.SelectedIndex == 0 ? "Succeeded" : "Failed";
                payment = await client.SimulateAsync(payment.Id, outcome);
            }

            await DisplayPaymentStatus(payment);
        }
        catch (HttpRequestException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Service de paiement indisponible", "Impossible de joindre le backend. Vérifiez qu’il est démarré et que l’adresse réseau est correcte.", "OK");
        }
        catch (JsonException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Réponse invalide", "Le backend a renvoyé une réponse illisible. Consultez les journaux du serveur.", "OK");
        }
        catch (InvalidDataException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Paiement impossible", exception.Message, "OK");
        }
        finally
        {
            payButton.IsEnabled = true;
            spinner.IsVisible = spinner.IsRunning = false;
        }
    }

    async void OnRefreshPayment(object? sender, EventArgs e)
    {
        if (pendingPaymentId is null)
            return;

        payButton.IsEnabled = false;
        spinner.IsVisible = spinner.IsRunning = true;
        try
        {
            var client = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<PaymentApiClient>();
            var payment = await client.GetPaymentAsync(pendingPaymentId.Value);
            await DisplayPaymentStatus(payment);
        }
        catch (HttpRequestException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Service de paiement indisponible", "Impossible de récupérer le statut auprès du backend.", "OK");
        }
        catch (JsonException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync("Réponse invalide", "Le backend a renvoyé une réponse illisible.", "OK");
        }
        finally
        {
            payButton.IsEnabled = true;
            spinner.IsVisible = spinner.IsRunning = false;
        }
    }

    async Task DisplayPaymentStatus(PaymentApiResponse payment)
    {
        switch (payment.Status)
        {
            case "Pending":
                pendingPaymentId = payment.Id;
                refreshPaymentButton.IsVisible = true;
                await DisplayAlertAsync("Paiement en attente", $"Référence : {payment.Id}\nVous pouvez actualiser le statut après la réponse de l’opérateur simulé.", "OK");
                break;
            case "Failed":
                pendingPaymentId = null;
                refreshPaymentButton.IsVisible = false;
                await DisplayAlertAsync("Paiement refusé", $"Le paiement de {Fmt.Fcfa(payment.Amount)} a été refusé par l’opérateur simulé.", "OK");
                break;
            case "Succeeded":
                pendingPaymentId = null;
                refreshPaymentButton.IsVisible = false;
                if (payment.Purpose == "Subscription")
                {
                    Session.CreateContract();
                    await OfferTransactionReceipt(payment);
                    await Shell.Current.GoToAsync("//main/catalog");
                    await Shell.Current.GoToAsync("//main/contracts");
                    await Shell.Current.GoToAsync(nameof(CertificatePage));
                }
                else
                {
                    Session.PayNextInstallment();
                    await OfferTransactionReceipt(payment);
                    await Shell.Current.GoToAsync("..");
                }
                break;
            default:
                throw new InvalidDataException($"Statut de paiement inconnu : {payment.Status}.");
        }
    }

    async Task OfferTransactionReceipt(PaymentApiResponse payment)
    {
        var choice = await DisplayActionSheetAsync(
            $"Paiement réussi — {Fmt.Fcfa(payment.Amount)}",
            "Continuer sans télécharger",
            null,
            "Télécharger le reçu de transaction");

        if (choice != "Télécharger le reçu de transaction")
            return;

        try
        {
            await TransactionReceiptService.CreateAndShareAsync(payment);
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await DisplayAlertAsync(
                "Téléchargement impossible",
                "Le reçu n’a pas pu être préparé ou partagé. Vérifiez l’espace disponible puis réessayez.",
                "OK");
        }
    }
}
