using MicroAssur.Models;
using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class CatalogPage : ContentPage
{
    public CatalogPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(categoriesLayout, Catalog.All);
    }

    async void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is Category category)
        {
            Session.Category = category;
            Session.Plan = null;
            await Shell.Current.GoToAsync(nameof(CategoryPage));
        }
    }
}
