using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class ContractsPage : ContentPage
{
    public ContractsPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var c = Session.ActiveContract;
        if (c is null) return;

        contractTitle.Text = c.Title;
        contractNumber.Text = $"N° {c.Number}";
        contractPrice.Text = c.PriceText;
        categoryValue.Text = c.CategoryName;
        planValue.Text = c.PlanName;
        startValue.Text = Fmt.Date(c.StartDate);
        nextValue.Text = c.NextDueText;
    }

    async void OnScheduleTapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(SchedulePage));

    async void OnCertificate(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(CertificatePage));

    async void OnClaim(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ClaimPage));
}
