using System.Collections.ObjectModel;
using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class ClaimPage : ContentPage
{
    int step = 1;
    readonly ObservableCollection<string> files = new();

    public ClaimPage()
    {
        InitializeComponent();
        typePicker.ItemsSource = new[] { "Accident", "Vol", "Incendie", "Dommages matériels", "Autre" };
        typePicker.SelectedIndex = 0;
        datePicker.MaximumDate = DateTime.Today;
        datePicker.Date = DateTime.Today;
        BindableLayout.SetItemsSource(filesLayout, files);
        ShowStep();
    }

    static Color Res(string key) => (Color)Application.Current!.Resources[key];

    void ShowStep()
    {
        var panels = new View[] { panel1, panel2, panel3, panel4 };
        var circles = new[] { c1, c2, c3, c4 };
        var numbers = new[] { n1, n2, n3, n4 };
        var labels = new[] { l1, l2, l3, l4 };

        for (int i = 0; i < 4; i++)
        {
            int idx = i + 1;
            panels[i].IsVisible = idx == step;
            circles[i].BackgroundColor = idx <= step ? Res("Primary") : Color.FromArgb("#B7D3F0");
            numbers[i].Text = idx < step ? "✓" : idx.ToString();
            labels[i].TextColor = idx <= step ? Res("TextDark") : Res("TextMuted");
        }

        nextButton.Text = step == 4 ? "Envoyer" : "Suivant";
        backButton.IsVisible = step > 1;
        if (step == 4) BuildRecap();
    }

    string SelectedType => typePicker.SelectedItem as string ?? "Accident";

    int Estimate => int.TryParse(new string((estimateEntry.Text ?? "").Where(char.IsDigit).ToArray()), out var v) ? v : 0;

    void BuildRecap()
    {
        recapLabel.Text =
            $"Type : {SelectedType}\n" +
            $"Date : {Fmt.Date(datePicker.Date ?? DateTime.Today)}\n" +
            $"Lieu : {placeEntry.Text?.Trim()}\n" +
            $"Estimation : {(Estimate > 0 ? Fmt.Fcfa(Estimate) : "non précisée")}\n" +
            $"Pièces jointes : {files.Count}\n\n" +
            $"Description :\n{descEditor.Text?.Trim()}";
    }

    async void OnAddFile(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choisir un document ou une photo"
            });
            if (result is not null) files.Add(result.FileName);
        }
        catch (Exception)
        {
            await DisplayAlert("Fichier", "Impossible d’ajouter ce fichier.", "OK");
        }
    }

    void OnBack(object? sender, EventArgs e)
    {
        if (step > 1) { step--; ShowStep(); }
    }

    async void OnNext(object? sender, EventArgs e)
    {
        if (step == 1 && string.IsNullOrWhiteSpace(descEditor.Text))
        {
            await DisplayAlert("Description requise", "Décrivez brièvement ce qui s’est passé.", "OK");
            return;
        }
        if (step == 2 && string.IsNullOrWhiteSpace(placeEntry.Text))
        {
            await DisplayAlert("Lieu requis", "Indiquez le lieu du sinistre.", "OK");
            return;
        }

        if (step < 4)
        {
            step++;
            ShowStep();
            return;
        }

        // Envoi (simulation)
        var claim = new Claim
        {
            Number = $"SIN-{DateTime.Today.Year}-{Random.Shared.Next(10000, 99999)}",
            Type = SelectedType,
            CategoryName = Session.ActiveContract?.CategoryName ?? "Accident / Moto",
            IncidentDate = datePicker.Date ?? DateTime.Today,
            Place = placeEntry.Text?.Trim() ?? "",
            Description = descEditor.Text?.Trim() ?? "",
            Estimate = Estimate,
            CreatedAt = DateTime.Now
        };
        Session.CurrentClaim = claim;

        await DisplayAlert("Déclaration envoyée", $"Votre dossier {claim.Number} a bien été enregistré.", "OK");
        await Shell.Current.GoToAsync($"../{nameof(ClaimTrackingPage)}");
    }
}

