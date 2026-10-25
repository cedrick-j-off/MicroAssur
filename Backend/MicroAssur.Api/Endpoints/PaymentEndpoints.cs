using MicroAssur.Api.Contracts;
using MicroAssur.Api.Models;
using MicroAssur.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MicroAssur.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var payments = app.MapGroup("/api/payments").WithTags("Paiements");

        payments.MapGet("/", async Task<Ok<IReadOnlyList<PaymentResponse>>> (
                IPaymentService service, CancellationToken cancellationToken) =>
                TypedResults.Ok((IReadOnlyList<PaymentResponse>)(await service.GetAllAsync(cancellationToken))
                    .Select(ToResponse).ToArray()))
            .WithName("ListPayments")
            .WithSummary("Liste les paiements, du plus récent au plus ancien.")
            .Produces<IReadOnlyList<PaymentResponse>>(StatusCodes.Status200OK);

        payments.MapGet("/{id:guid}", async Task<Ok<PaymentResponse>> (
                Guid id, IPaymentService service, CancellationToken cancellationToken) =>
                TypedResults.Ok(ToResponse(await service.GetAsync(id, cancellationToken))))
            .WithName("GetPayment")
            .WithSummary("Retourne l’état courant d’un paiement.")
            .Produces<PaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        payments.MapPost("/", async Task<Created<PaymentResponse>> (
                CreatePaymentRequest request,
                HttpRequest httpRequest,
                IPaymentService service,
                CancellationToken cancellationToken) =>
            {
                var key = httpRequest.Headers["Idempotency-Key"].ToString();
                var payment = await service.CreateAsync(request, key, cancellationToken);
                return TypedResults.Created($"/api/payments/{payment.Id}", ToResponse(payment));
            })
            .WithName("CreatePayment")
            .WithSummary("Démarre un paiement MTN ou Orange en état en attente.")
            .WithDescription("Tous les paiements sont simulés : aucun débit réel n’est effectué. Fournir une clé Idempotency-Key unique.")
            .Produces<PaymentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        if (app.Environment.IsDevelopment())
        {
            var simulator = app.MapGroup("/api/simulator/payments")
                .WithTags("Simulateur (développement uniquement)");

            simulator.MapPost("/{id:guid}/status", async Task<Ok<PaymentResponse>> (
                    Guid id,
                    SimulatePaymentRequest request,
                    IPaymentService service,
                    CancellationToken cancellationToken) =>
                    TypedResults.Ok(ToResponse(await service.SimulateAsync(id, request.Outcome, cancellationToken))))
                .WithName("SimulatePaymentStatus")
                .WithSummary("Simule la réponse d’un opérateur Mobile Money.")
                .WithDescription("Route disponible uniquement en environnement Development : confirme, refuse ou laisse en attente le paiement.")
                .Produces<PaymentResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }

    private static PaymentResponse ToResponse(PaymentRecord payment) => new(
        payment.Id,
        payment.Provider,
        payment.Purpose,
        payment.Status,
        payment.Amount,
        payment.Currency,
        payment.PhoneNumber,
        payment.SubscriptionId,
        payment.CreatedAt,
        payment.UpdatedAt);
}
