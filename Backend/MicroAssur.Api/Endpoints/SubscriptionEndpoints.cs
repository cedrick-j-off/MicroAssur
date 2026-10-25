using MicroAssur.Api.Contracts;
using MicroAssur.Api.Models;
using MicroAssur.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MicroAssur.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this WebApplication app)
    {
        app.MapGet("/api/plans", (IPaymentService service) =>
                TypedResults.Ok(service.GetPlans().Select(plan =>
                    new PlanResponse(plan.CategoryId, plan.CategoryName, plan.PlanName, plan.MonthlyAmount)).ToArray()))
            .WithName("ListPlans")
            .WithTags("Catalogue")
            .WithSummary("Liste les formules et leurs montants mensuels en FCFA.")
            .Produces<PlanResponse[]>(StatusCodes.Status200OK);

        var subscriptions = app.MapGroup("/api/subscriptions").WithTags("Contrats");
        subscriptions.MapGet("/", async Task<Ok<IReadOnlyList<SubscriptionResponse>>> (
                IPaymentService service, CancellationToken cancellationToken) =>
                TypedResults.Ok((IReadOnlyList<SubscriptionResponse>)(await service.GetSubscriptionsAsync(cancellationToken))
                    .Select(ToResponse).ToArray()))
            .WithName("ListSubscriptions")
            .WithSummary("Liste les contrats créés après un paiement confirmé.")
            .Produces<IReadOnlyList<SubscriptionResponse>>(StatusCodes.Status200OK);

        subscriptions.MapGet("/{id:guid}", async Task<Ok<SubscriptionResponse>> (
                Guid id, IPaymentService service, CancellationToken cancellationToken) =>
                TypedResults.Ok(ToResponse(await service.GetSubscriptionAsync(id, cancellationToken))))
            .WithName("GetSubscription")
            .WithSummary("Retourne un contrat créé par un paiement confirmé.")
            .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static SubscriptionResponse ToResponse(SubscriptionRecord subscription) => new(
        subscription.Id,
        subscription.InitialPaymentId,
        subscription.CustomerName,
        subscription.PhoneNumber,
        subscription.BirthDate,
        subscription.Address,
        subscription.CategoryId,
        subscription.CategoryName,
        subscription.PlanName,
        subscription.MonthlyAmount,
        subscription.CreatedAt);
}
