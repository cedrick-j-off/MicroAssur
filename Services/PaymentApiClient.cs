using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroAssur.Services;

public sealed class PaymentApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<PaymentApiResponse> CreatePaymentAsync(
        CreatePaymentApiRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/payments")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey);
        using var response = await httpClient.SendAsync(message, cancellationToken);
        return await ReadResponseAsync<PaymentApiResponse>(response, cancellationToken);
    }

    public async Task<PaymentApiResponse> SimulateAsync(
        Guid paymentId,
        string outcome,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/simulator/payments/{paymentId}/status",
            new SimulatePaymentApiRequest(outcome),
            JsonOptions,
            cancellationToken);
        return await ReadResponseAsync<PaymentApiResponse>(response, cancellationToken);
    }

    public async Task<PaymentApiResponse> GetPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/payments/{paymentId}", cancellationToken);
        return await ReadResponseAsync<PaymentApiResponse>(response, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"L’API a répondu {(int)response.StatusCode} ({response.ReasonPhrase}). {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new JsonException("L’API a retourné une réponse vide ou invalide.");
    }
}

public sealed record CreatePaymentApiRequest
{
    public required string Provider { get; init; }
    public required string Purpose { get; init; }
    public required int Amount { get; init; }
    public required string PhoneNumber { get; init; }
    public required string CustomerName { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? Address { get; init; }
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? PlanName { get; init; }
}

public sealed record SimulatePaymentApiRequest(string Outcome);

public sealed record PaymentApiResponse(
    Guid Id,
    string Provider,
    string Purpose,
    string Status,
    int Amount,
    string Currency,
    string PhoneNumber,
    Guid? SubscriptionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
