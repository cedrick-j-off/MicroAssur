using MicroAssur.Views;

namespace MicroAssur;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages « poussées » (hors onglets)
        Routing.RegisterRoute(nameof(CategoryPage), typeof(CategoryPage));         // 03
        Routing.RegisterRoute(nameof(GuaranteesPage), typeof(GuaranteesPage));     // 04
        Routing.RegisterRoute(nameof(PlansPage), typeof(PlansPage));               // 05
        Routing.RegisterRoute(nameof(PersonalInfoPage), typeof(PersonalInfoPage)); // 06
        Routing.RegisterRoute(nameof(SummaryPage), typeof(SummaryPage));           // 07
        Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));           // 08
        Routing.RegisterRoute(nameof(CertificatePage), typeof(CertificatePage));   // 09/10
        Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));         // 12
        Routing.RegisterRoute(nameof(ClaimPage), typeof(ClaimPage));               // 13
        Routing.RegisterRoute(nameof(ClaimTrackingPage), typeof(ClaimTrackingPage)); // 14
        Routing.RegisterRoute(nameof(CompensationPage), typeof(CompensationPage)); // 15
    }
}
