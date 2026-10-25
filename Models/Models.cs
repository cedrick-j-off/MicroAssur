using System.Globalization;
using Microsoft.Maui.Graphics;

namespace MicroAssur.Models;

/// <summary>Formatage FR (FCFA, dates) sans dépendre de la culture du téléphone.</summary>
public static class Fmt
{
    static readonly string[] Months =
    {
        "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"
    };

    public static string Fcfa(int amount) =>
        amount.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', ' ') + " FCFA";

    public static string MonthYear(DateTime d) => $"{Months[d.Month - 1]} {d.Year}";

    public static string Date(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string DateTimeFr(DateTime d) => d.ToString("dd/MM/yyyy - HH'h'mm", CultureInfo.InvariantCulture);
}

// ───────────── Catalogue ─────────────

public record Guarantee(string Icon, string Title, string Limit);

public class Plan
{
    public Plan(string name, int priceFcfa, string tagline, bool recommended = false)
    {
        Name = name;
        PriceFcfa = priceFcfa;
        Tagline = tagline;
        Recommended = recommended;
    }

    public string Name { get; }
    public int PriceFcfa { get; }
    public string Tagline { get; }
    public bool Recommended { get; }

    public string PriceText => $"{Fmt.Fcfa(PriceFcfa)}/mois";
    public Brush StrokeBrush => new SolidColorBrush(Color.FromArgb(Recommended ? "#0A84FF" : "#D3E5F7"));
    public double StrokeWidth => Recommended ? 2 : 1;
}

public class Category
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Icon { get; init; } = "";
    public Color Tint { get; init; } = Colors.White;
    public string Hero { get; init; } = "logo.png";
    public string Intro { get; init; } = "";
    public List<string> Advantages { get; init; } = new();
    public List<Guarantee> Guarantees { get; init; } = new();
    public List<string> Exclusions { get; init; } = new();
    public List<string> Indemnisation { get; init; } = new();
    public List<Plan> Plans { get; init; } = new();
}

// ───────────── Utilisateur ─────────────

public class UserInfo
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime BirthDate { get; set; } = new DateTime(1990, 1, 1);
    public string Address { get; set; } = "";

    public static UserInfo Demo() => new()
    {
        FullName = "Jean Mbarga",
        Phone = "+237 6 12 34 56 78",
        BirthDate = new DateTime(1990, 1, 1),
        Address = "Yaoundé, Cameroun"
    };
}

// ───────────── Contrats ─────────────

public class Installment
{
    public DateTime DueDate { get; set; }
    public int Amount { get; set; }
    public bool Paid { get; set; }
    public bool ShowLine { get; set; } = true;

    public string Label => Fmt.MonthYear(DueDate);
    public string AmountText => Fmt.Fcfa(Amount);
    public string StatusText => Paid ? "Payé" : "À venir";
    public Color StatusBg => Color.FromArgb(Paid ? "#DDF4E8" : "#E8F2FD");
    public Color StatusFg => Color.FromArgb(Paid ? "#1FAA6B" : "#0058D6");
    public Color DotColor => Color.FromArgb(Paid ? "#1FAA6B" : "#0058D6");
}

public class Contract
{
    public string Number { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string PlanName { get; set; } = "";
    public int PriceFcfa { get; set; }
    public DateTime StartDate { get; set; }
    public List<Installment> Installments { get; set; } = new();

    public string Title => $"{CategoryName} - {PlanName}";
    public string PriceText => $"{Fmt.Fcfa(PriceFcfa)}/mois";
    public Installment? NextInstallment => Installments.FirstOrDefault(i => !i.Paid);
    public string NextDueText => NextInstallment is null ? "—" : Fmt.Date(NextInstallment.DueDate);
    public string StatusText => NextInstallment is null ? "Cotisations réglées" : "En cours";

    public static Contract Demo() => new()
    {
        Number = "MA-2025-00123",
        CategoryName = "Accident / Moto",
        PlanName = "Confort",
        PriceFcfa = 1000,
        StartDate = new DateTime(2025, 10, 15),
        Installments = new List<Installment>
        {
            new() { DueDate = new DateTime(2025, 11, 1), Amount = 1000, Paid = true },
            new() { DueDate = new DateTime(2025, 12, 1), Amount = 1000 },
            new() { DueDate = new DateTime(2026, 1, 1), Amount = 1000 },
            new() { DueDate = new DateTime(2026, 2, 1), Amount = 1000 },
            new() { DueDate = new DateTime(2026, 3, 1), Amount = 1000, ShowLine = false },
        }
    };

    public static Contract Create(Category category, Plan plan)
    {
        var start = DateTime.Today;
        var list = new List<Installment>();
        for (int i = 0; i < 6; i++)
            list.Add(new Installment { DueDate = start.AddMonths(i), Amount = plan.PriceFcfa, Paid = i == 0, ShowLine = i < 5 });

        return new Contract
        {
            Number = $"MA-{start.Year}-{Random.Shared.Next(10000, 99999)}",
            CategoryName = category.Name,
            PlanName = plan.Name,
            PriceFcfa = plan.PriceFcfa,
            StartDate = start,
            Installments = list
        };
    }
}

// ───────────── Sinistres ─────────────

public enum StepState { Done, Current, Pending }

public class TimelineStep
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public StepState State { get; set; }
    public bool ShowLine { get; set; } = true;

    public Color CircleBg => State switch
    {
        StepState.Done => Color.FromArgb("#1FAA6B"),
        StepState.Current => Color.FromArgb("#0058D6"),
        _ => Color.FromArgb("#CFE0F3")
    };
    public string Glyph => State == StepState.Pending ? "●" : "✓";
    public Color GlyphColor => State == StepState.Pending ? Color.FromArgb("#0B3A8D") : Colors.White;
    public Color TitleColor => State switch
    {
        StepState.Done => Color.FromArgb("#1FAA6B"),
        StepState.Current => Color.FromArgb("#0058D6"),
        _ => Color.FromArgb("#4A82BE")
    };
    public FontAttributes TitleWeight => State == StepState.Current ? FontAttributes.Bold : FontAttributes.None;
}

public class Claim
{
    public string Number { get; set; } = "";
    public string Type { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public DateTime IncidentDate { get; set; }
    public string Place { get; set; } = "";
    public string Description { get; set; } = "";
    public int Estimate { get; set; }
    public int Payout { get; set; } = 500000;
    public DateTime CreatedAt { get; set; }
    public DateTime? AnalysisAt { get; set; }

    public static Claim Demo() => new()
    {
        Number = "SIN-2025-00456",
        Type = "Accident",
        CategoryName = "Accident / Moto",
        IncidentDate = new DateTime(2025, 10, 15),
        CreatedAt = new DateTime(2025, 10, 15, 10, 24, 0),
        AnalysisAt = new DateTime(2025, 10, 16, 14, 32, 0)
    };

    public List<TimelineStep> BuildTimeline() => new()
    {
        new() { Title = "Déclaration reçue", Subtitle = Fmt.DateTimeFr(CreatedAt), State = StepState.Done },
        new() { Title = "En cours d’analyse", Subtitle = AnalysisAt is null ? "En cours de traitement" : Fmt.DateTimeFr(AnalysisAt.Value), State = StepState.Current },
        new() { Title = "Évaluation des dommages", Subtitle = "En attente", State = StepState.Pending },
        new() { Title = "Indemnisation", Subtitle = "En attente", State = StepState.Pending, ShowLine = false },
    };
}
