using MicroAssur.Api.Models;

namespace MicroAssur.Api.Services;

public interface IPaymentStore
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<PaymentRecord?> GetPaymentAsync(Guid id, CancellationToken cancellationToken);
    Task<PaymentRecord?> GetPaymentByKeyAsync(string key, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentRecord>> GetPaymentsAsync(CancellationToken cancellationToken);
    Task<PaymentRecord> CreatePaymentAsync(PaymentRecord payment, CancellationToken cancellationToken);
    Task<PaymentRecord> ApplySimulationAsync(Guid id, SimulationOutcome outcome, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubscriptionRecord>> GetSubscriptionsAsync(CancellationToken cancellationToken);
    Task<SubscriptionRecord?> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken);
}
