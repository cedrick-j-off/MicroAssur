using System.Globalization;
using System.Text;

namespace MicroAssur.Services;

public static class TransactionReceiptService
{
    public static async Task CreateAndShareAsync(PaymentApiResponse payment)
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "transactions");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"transaction-{payment.Id:N}.txt");
        var provider = payment.Provider switch
        {
            "Mtn" => "MTN Mobile Money",
            "Orange" => "Orange Money",
            _ => payment.Provider
        };
        var purpose = payment.Purpose switch
        {
            "Subscription" => "Première cotisation",
            "Installment" => "Échéance",
            _ => payment.Purpose
        };
        var status = payment.Status switch
        {
            "Succeeded" => "Réussi",
            "Failed" => "Refusé",
            "Pending" => "En attente",
            _ => payment.Status
        };
        var date = payment.UpdatedAt.ToLocalTime().ToString(
            "dd/MM/yyyy 'à' HH:mm",
            CultureInfo.GetCultureInfo("fr-FR"));

        var lines = new List<string>
        {
            "MICROASSUR — REÇU DE TRANSACTION",
            "=================================",
            "",
            $"Référence : {payment.Id}",
            $"Date : {date}",
            $"Statut : {status}",
            $"Opérateur : {provider}",
            $"Téléphone : {payment.PhoneNumber}",
            $"Type : {purpose}",
            $"Montant : {payment.Amount.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"))} FCFA"
        };

        if (payment.SubscriptionId is not null)
            lines.Add($"Contrat : {payment.SubscriptionId}");
        lines.Add(string.Empty);
        lines.Add("Paiement de démonstration : aucun débit réel n’a été effectué.");

        var content = string.Join(Environment.NewLine, lines);
        await File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = $"Reçu de transaction {payment.Id}",
            File = new ShareFile(path, "text/plain")
        });
    }
}
