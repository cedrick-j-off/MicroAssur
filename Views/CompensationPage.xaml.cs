using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class CompensationPage : ContentPage
{
    readonly Claim claim = Session.CurrentClaim ?? Claim.Demo();

    public CompensationPage()
    {
        InitializeComponent();
        messageLabel.Text =
            $"Votre dossier a été validé. Le montant de {Fmt.Fcfa(claim.Payout)} sera versé sur votre compte Mobile Money dans les prochaines 48h.";
    }

    async void OnDetail(object? sender, EventArgs e) =>
        await DisplayAlert("Détail de l’indemnisation",
            $"Dossier : {claim.Number}\nMontant : {Fmt.Fcfa(claim.Payout)}\nMode de versement : Mobile Money\nDélai estimé : 48 h",
            "OK");
}
