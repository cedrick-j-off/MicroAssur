using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class CategoryPage : ContentPage
{
    public CategoryPage()
    {
        InitializeComponent();
        var c = Session.CurrentCategory;
        hero.Source = c.Hero;
        titleLabel.Text = c.Name;
        introLabel.Text = c.Intro;
        BindableLayout.SetItemsSource(advantagesLayout, c.Advantages);
    }

    async void OnFormulas(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(GuaranteesPage));
}
