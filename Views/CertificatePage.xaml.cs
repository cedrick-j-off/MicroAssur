using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class CertificatePage : ContentPage
{
    public CertificatePage() => InitializeComponent();

    static string Number => Session.ActiveContract?.Number ?? "MA-2025-00123";

    async void OnDownload(object? sender, EventArgs e) =>
        await DisplayAlert("Attestation", $"L’attestation du contrat {Number} a été téléchargée (simulation).", "OK");

    async void OnShare(object? sender, EventArgs e) =>
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Attestation MicroAssur",
            Text = $"Attestation d’assurance MicroAssur – contrat {Number}"
        });
}
