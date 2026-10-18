using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class ClaimTrackingPage : ContentPage
{
    public ClaimTrackingPage()
    {
        InitializeComponent();
        var claim = Session.CurrentClaim ?? Claim.Demo();
        claimNumber.Text = $"Sinistre #{claim.Number}";
        claimCategory.Text = claim.CategoryName;
        BindableLayout.SetItemsSource(stepsLayout, claim.BuildTimeline());
    }

    async void OnCompensation(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(CompensationPage));
}
