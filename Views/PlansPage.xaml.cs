using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class PlansPage : ContentPage
{
    public PlansPage()
    {
        InitializeComponent();
        var c = Session.CurrentCategory;
        Title = c.Name;
        BindableLayout.SetItemsSource(plansLayout, c.Plans);
    }

    async void OnPlanTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Plan plan)
        {
            Session.Plan = plan;
            await Shell.Current.GoToAsync(nameof(PersonalInfoPage));
        }
    }
}
