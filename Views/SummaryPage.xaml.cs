using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class SummaryPage : ContentPage
{
    public SummaryPage()
    {
        InitializeComponent();
        categoryValue.Text = Session.CurrentCategory.Name;
        planValue.Text = Session.CurrentPlan.Name;
        priceValue.Text = $"{Fmt.Fcfa(Session.CurrentPlan.PriceFcfa)} / mois";
    }

    async void OnSeeGuarantees(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(GuaranteesPage));

    async void OnPay(object? sender, EventArgs e)
    {
        Session.PayMode = PayMode.Subscription;
        await Shell.Current.GoToAsync(nameof(PaymentPage));
    }
}
