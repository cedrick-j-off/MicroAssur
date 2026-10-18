using MicroAssur.Services;

namespace MicroAssur.Views;

public partial class GuaranteesPage : ContentPage
{
    public GuaranteesPage()
    {
        InitializeComponent();
        var c = Session.CurrentCategory;
        Title = c.Name;
        BindableLayout.SetItemsSource(panelG, c.Guarantees);
        BindableLayout.SetItemsSource(panelE, c.Exclusions);
        BindableLayout.SetItemsSource(panelI, c.Indemnisation);
        Select(0);
    }

    static Color Res(string key) => (Color)Application.Current!.Resources[key];

    void Select(int index)
    {
        var chips = new[] { chipG, chipE, chipI };
        var labels = new[] { lblG, lblE, lblI };
        var panels = new View[] { panelG, panelE, panelI };
        var titles = new[] { "Garanties principales", "Exclusions", "Indemnisation" };

        for (int i = 0; i < 3; i++)
        {
            bool on = i == index;
            chips[i].BackgroundColor = on ? Res("Primary") : Res("Chip");
            labels[i].TextColor = on ? Colors.White : Res("TextMuted");
            panels[i].IsVisible = on;
        }
        sectionTitle.Text = titles[index];
    }

    void OnChipG(object? sender, TappedEventArgs e) => Select(0);
    void OnChipE(object? sender, TappedEventArgs e) => Select(1);
    void OnChipI(object? sender, TappedEventArgs e) => Select(2);

    async void OnContinue(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(PlansPage));
}
