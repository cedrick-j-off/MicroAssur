using MicroAssur.Api.Models;

namespace MicroAssur.Api.Services;

public interface IPaymentAggregator
{
    Task<PaymentRecord> InitiateAsync(PaymentRecord payment, CancellationToken cancellationToken);
}
