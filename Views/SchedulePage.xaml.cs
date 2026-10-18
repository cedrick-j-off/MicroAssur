using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class SchedulePage : ContentPage
{
    public SchedulePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var c = Session.ActiveContract;
        if (c is null) return;

        contractLabel.Text = $"Contrat {c.Number}";
        priceLabel.Text = c.PriceText;
        BindableLayout.SetItemsSource(installmentsLayout, null);
        BindableLayout.SetItemsSource(installmentsLayout, c.Installments.ToList());
        payButton.IsVisible = c.NextInstallment is not null;
    }

    async void OnPayNext(object? sender, EventArgs e)
    {
        Session.PayMode = PayMode.Installment;
        await Shell.Current.GoToAsync(nameof(PaymentPage));
    }
}
