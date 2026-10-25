using MicroAssur.Api.Contracts;
using MicroAssur.Api.Models;

namespace MicroAssur.Api.Services;

public interface IPaymentService
{
    IReadOnlyList<PaymentPlan> GetPlans();
    Task<PaymentRecord> CreateAsync(CreatePaymentRequest request, string idempotencyKey, CancellationToken cancellationToken);
    Task<PaymentRecord> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentRecord>> GetAllAsync(CancellationToken cancellationToken);
    Task<PaymentRecord> SimulateAsync(Guid id, SimulationOutcome outcome, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubscriptionRecord>> GetSubscriptionsAsync(CancellationToken cancellationToken);
    Task<SubscriptionRecord> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken);
}
