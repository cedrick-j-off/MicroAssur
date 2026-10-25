using MicroAssur.Api.Models;

namespace MicroAssur.Api.Services;

public sealed class SimulatedPaymentAggregator(IPaymentStore store) : IPaymentAggregator
{
    public Task<PaymentRecord> InitiateAsync(PaymentRecord payment, CancellationToken cancellationToken) =>
        store.CreatePaymentAsync(payment, cancellationToken);
}
