using System.ComponentModel.DataAnnotations;
using MicroAssur.Api.Models;

namespace MicroAssur.Api.Contracts;

/// <summary>Informations requises pour initier un paiement Mobile Money simulé.</summary>
public sealed record CreatePaymentRequest
{
    [Required]
    public required PaymentProvider Provider { get; init; }

    [Required]
    public required PaymentPurpose Purpose { get; init; }

    [Range(1, 100000000)]
    public required int Amount { get; init; }

    [Required, RegularExpression(@"^\+?[0-9][0-9\s-]{7,18}$")]
    public required string PhoneNumber { get; init; }

    [Required, MinLength(3), MaxLength(160)]
    public required string CustomerName { get; init; }

    public DateOnly? BirthDate { get; init; }

    [MaxLength(250)]
    public string? Address { get; init; }

    [MaxLength(40)]
    public string? CategoryId { get; init; }

    [MaxLength(80)]
    public string? CategoryName { get; init; }

    [MaxLength(80)]
    public string? PlanName { get; init; }
}

/// <summary>Résultat contrôlé par le simulateur de paiement.</summary>
public sealed record SimulatePaymentRequest
{
    [Required]
    public required SimulationOutcome Outcome { get; init; }
}

/// <summary>Détails d’un paiement et de son état chez l’agrégateur simulé.</summary>
public sealed record PaymentResponse(
    Guid Id,
    PaymentProvider Provider,
    PaymentPurpose Purpose,
    PaymentStatus Status,
    int Amount,
    string Currency,
    string PhoneNumber,
    Guid? SubscriptionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>Contrat créé après confirmation du premier paiement.</summary>
public sealed record SubscriptionResponse(
    Guid Id,
    Guid InitialPaymentId,
    string CustomerName,
    string PhoneNumber,
    DateOnly BirthDate,
    string Address,
    string CategoryId,
    string CategoryName,
    string PlanName,
    int MonthlyAmount,
    DateTimeOffset CreatedAt);

/// <summary>Formule disponible pour une catégorie d’assurance.</summary>
public sealed record PlanResponse(string CategoryId, string CategoryName, string PlanName, int MonthlyAmount);
