using MicroAssur.Api.Contracts;
using MicroAssur.Api.Models;

namespace MicroAssur.Api.Services;

public sealed class PaymentService(
    IPaymentStore store,
    IPaymentAggregator aggregator) : IPaymentService
{
    private static readonly PaymentPlan[] Plans =
    [
        new("sante", "Santé", "Essentiel", 1000),
        new("sante", "Santé", "Confort", 2000),
        new("sante", "Santé", "Sérénité", 3000),
        new("moto", "Accident / Moto", "Essentiel", 500),
        new("moto", "Accident / Moto", "Confort", 1000),
        new("moto", "Accident / Moto", "Sérénité", 1500),
        new("incendie", "Incendie & Vol", "Essentiel", 1000),
        new("incendie", "Incendie & Vol", "Confort", 2000),
        new("incendie", "Incendie & Vol", "Sérénité", 3500),
        new("vehicule", "Moto / Véhicule", "Essentiel", 1500),
        new("vehicule", "Moto / Véhicule", "Confort", 2500),
        new("vehicule", "Moto / Véhicule", "Sérénité", 4000),
        new("deces", "Décès & Obsèques", "Essentiel", 500),
        new("deces", "Décès & Obsèques", "Confort", 1000),
        new("deces", "Décès & Obsèques", "Sérénité", 2000)
    ];

    public IReadOnlyList<PaymentPlan> GetPlans() => Plans;

    public async Task<PaymentRecord> CreateAsync(
        CreatePaymentRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new ArgumentException("L’en-tête Idempotency-Key est obligatoire et limité à 100 caractères.");
        if (!Enum.IsDefined(request.Provider) || !Enum.IsDefined(request.Purpose))
            throw new ArgumentException("L’opérateur ou le type de paiement n’est pas pris en charge.");

        if (request.Purpose == PaymentPurpose.Subscription)
        {
            var plan = Plans.SingleOrDefault(candidate =>
                string.Equals(candidate.CategoryId, request.CategoryId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.PlanName, request.PlanName, StringComparison.OrdinalIgnoreCase));

            if (plan is null)
                throw new ArgumentException("La catégorie et la formule sélectionnées sont inconnues.");
            if (plan.MonthlyAmount != request.Amount
                || !string.Equals(plan.CategoryName, request.CategoryName, StringComparison.Ordinal))
                throw new ArgumentException("Le montant et la catégorie doivent correspondre à la formule sélectionnée.");
            if (request.BirthDate is null || request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
                throw new ArgumentException("La souscription est réservée aux personnes majeures.");
            if (string.IsNullOrWhiteSpace(request.Address))
                throw new ArgumentException("L’adresse est obligatoire pour créer le contrat.");
        }

        var existing = await store.GetPaymentByKeyAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (MatchesRequest(existing, request))
                return existing;
            throw new RequestConflictException("Cette clé d’idempotence a déjà été utilisée pour une autre demande.");
        }

        var now = DateTimeOffset.UtcNow;
        var payment = new PaymentRecord(
            Guid.NewGuid(),
            idempotencyKey,
            request.Provider,
            request.Purpose,
            PaymentStatus.Pending,
            request.Amount,
            "XAF",
            NormalizePhone(request.PhoneNumber),
            request.CustomerName.Trim(),
            request.BirthDate,
            request.Address?.Trim(),
            request.CategoryId,
            request.CategoryName,
            request.PlanName,
            null,
            now,
            now);

        try
        {
            return await aggregator.InitiateAsync(payment, cancellationToken);
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            existing = await store.GetPaymentByKeyAsync(idempotencyKey, cancellationToken);
            if (existing is not null && MatchesRequest(existing, request))
                return existing;
            throw new RequestConflictException("Cette clé d’idempotence est déjà utilisée.");
        }
    }

    public async Task<PaymentRecord> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await store.GetPaymentAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("Paiement introuvable.");

    public Task<IReadOnlyList<PaymentRecord>> GetAllAsync(CancellationToken cancellationToken) =>
        store.GetPaymentsAsync(cancellationToken);

    public Task<PaymentRecord> SimulateAsync(Guid id, SimulationOutcome outcome, CancellationToken cancellationToken) =>
        store.ApplySimulationAsync(id, outcome, cancellationToken);

    public Task<IReadOnlyList<SubscriptionRecord>> GetSubscriptionsAsync(CancellationToken cancellationToken) =>
        store.GetSubscriptionsAsync(cancellationToken);

    public async Task<SubscriptionRecord> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken) =>
        await store.GetSubscriptionAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("Contrat introuvable.");

    private static bool MatchesRequest(PaymentRecord payment, CreatePaymentRequest request) =>
        payment.Provider == request.Provider
        && payment.Purpose == request.Purpose
        && payment.Amount == request.Amount
        && payment.PhoneNumber == NormalizePhone(request.PhoneNumber)
        && payment.CustomerName == request.CustomerName.Trim()
        && payment.BirthDate == request.BirthDate
        && payment.Address == request.Address?.Trim()
        && payment.CategoryId == request.CategoryId
        && payment.CategoryName == request.CategoryName
        && payment.PlanName == request.PlanName;

    private static string NormalizePhone(string phone) =>
        new(phone.Where(character => char.IsDigit(character) || character == '+').ToArray());
}
