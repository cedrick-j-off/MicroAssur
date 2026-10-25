using System.Globalization;
using MicroAssur.Api.Models;
using Microsoft.Data.Sqlite;

namespace MicroAssur.Api.Services;

public sealed class SqlitePaymentStore(IConfiguration configuration, IHostEnvironment environment) : IPaymentStore
{
    private readonly string _connectionString = BuildConnectionString(configuration, environment.ContentRootPath);

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode = WAL;
            CREATE TABLE IF NOT EXISTS payments (
                id TEXT PRIMARY KEY,
                idempotency_key TEXT NOT NULL UNIQUE,
                provider TEXT NOT NULL,
                purpose TEXT NOT NULL,
                status TEXT NOT NULL,
                amount INTEGER NOT NULL CHECK (amount > 0),
                currency TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                customer_name TEXT NOT NULL,
                birth_date TEXT NULL,
                address TEXT NULL,
                category_id TEXT NULL,
                category_name TEXT NULL,
                plan_name TEXT NULL,
                subscription_id TEXT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS subscriptions (
                id TEXT PRIMARY KEY,
                initial_payment_id TEXT NOT NULL UNIQUE REFERENCES payments(id),
                customer_name TEXT NOT NULL,
                phone_number TEXT NOT NULL,
                birth_date TEXT NOT NULL,
                address TEXT NOT NULL,
                category_id TEXT NOT NULL,
                category_name TEXT NOT NULL,
                plan_name TEXT NOT NULL,
                monthly_amount INTEGER NOT NULL CHECK (monthly_amount > 0),
                created_at TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS ix_payments_created_at ON payments(created_at DESC);
            CREATE INDEX IF NOT EXISTS ix_subscriptions_created_at ON subscriptions(created_at DESC);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<PaymentRecord?> GetPaymentAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM payments WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadPayment(reader) : null;
    }

    public async Task<PaymentRecord?> GetPaymentByKeyAsync(string key, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM payments WHERE idempotency_key = $key";
        command.Parameters.AddWithValue("$key", key);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadPayment(reader) : null;
    }

    public async Task<IReadOnlyList<PaymentRecord>> GetPaymentsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM payments ORDER BY created_at DESC";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var payments = new List<PaymentRecord>();
        while (await reader.ReadAsync(cancellationToken))
            payments.Add(ReadPayment(reader));
        return payments;
    }

    public async Task<PaymentRecord> CreatePaymentAsync(PaymentRecord payment, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO payments (
                id, idempotency_key, provider, purpose, status, amount, currency, phone_number,
                customer_name, birth_date, address, category_id, category_name, plan_name,
                subscription_id, created_at, updated_at
            ) VALUES (
                $id, $key, $provider, $purpose, $status, $amount, $currency, $phone,
                $customer, $birthDate, $address, $categoryId, $categoryName, $planName,
                NULL, $created, $updated
            )
            """;
        AddPaymentParameters(command, payment);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return payment;
    }

    public async Task<PaymentRecord> ApplySimulationAsync(
        Guid id,
        SimulationOutcome outcome,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var payment = await GetPaymentInTransactionAsync(connection, transaction, id, cancellationToken)
            ?? throw new ResourceNotFoundException("Paiement introuvable.");

        var requestedStatus = outcome switch
        {
            SimulationOutcome.Pending => PaymentStatus.Pending,
            SimulationOutcome.Succeeded => PaymentStatus.Succeeded,
            SimulationOutcome.Failed => PaymentStatus.Failed,
            _ => throw new ArgumentException("Résultat de simulation non pris en charge.")
        };

        if (payment.Status != PaymentStatus.Pending && payment.Status != requestedStatus)
            throw new RequestConflictException("Un paiement déjà confirmé ou refusé ne peut pas être modifié.");

        var updatedAt = DateTimeOffset.UtcNow;
        Guid? subscriptionId = payment.SubscriptionId;
        if (requestedStatus == PaymentStatus.Succeeded && payment.Purpose == PaymentPurpose.Subscription && subscriptionId is null)
        {
            if (payment.BirthDate is null || string.IsNullOrWhiteSpace(payment.Address)
                || string.IsNullOrWhiteSpace(payment.CategoryId)
                || string.IsNullOrWhiteSpace(payment.CategoryName)
                || string.IsNullOrWhiteSpace(payment.PlanName))
                throw new RequestConflictException("Les informations de souscription sont incomplètes.");

            subscriptionId = Guid.NewGuid();
            await using var subscriptionCommand = connection.CreateCommand();
            subscriptionCommand.Transaction = transaction;
            subscriptionCommand.CommandText = """
                INSERT INTO subscriptions (
                    id, initial_payment_id, customer_name, phone_number, birth_date, address,
                    category_id, category_name, plan_name, monthly_amount, created_at
                ) VALUES (
                    $id, $paymentId, $customer, $phone, $birthDate, $address,
                    $categoryId, $categoryName, $planName, $amount, $createdAt
                )
                """;
            subscriptionCommand.Parameters.AddWithValue("$id", subscriptionId.Value.ToString());
            subscriptionCommand.Parameters.AddWithValue("$paymentId", payment.Id.ToString());
            subscriptionCommand.Parameters.AddWithValue("$customer", payment.CustomerName);
            subscriptionCommand.Parameters.AddWithValue("$phone", payment.PhoneNumber);
            subscriptionCommand.Parameters.AddWithValue("$birthDate", payment.BirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            subscriptionCommand.Parameters.AddWithValue("$address", payment.Address);
            subscriptionCommand.Parameters.AddWithValue("$categoryId", payment.CategoryId);
            subscriptionCommand.Parameters.AddWithValue("$categoryName", payment.CategoryName);
            subscriptionCommand.Parameters.AddWithValue("$planName", payment.PlanName);
            subscriptionCommand.Parameters.AddWithValue("$amount", payment.Amount);
            subscriptionCommand.Parameters.AddWithValue("$createdAt", updatedAt.ToString("O", CultureInfo.InvariantCulture));
            await subscriptionCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;
        updateCommand.CommandText = """
            UPDATE payments
            SET status = $status, subscription_id = $subscriptionId, updated_at = $updatedAt
            WHERE id = $id
            """;
        updateCommand.Parameters.AddWithValue("$status", requestedStatus.ToString());
        updateCommand.Parameters.AddWithValue("$subscriptionId", subscriptionId?.ToString() is { } value ? value : DBNull.Value);
        updateCommand.Parameters.AddWithValue("$updatedAt", updatedAt.ToString("O", CultureInfo.InvariantCulture));
        updateCommand.Parameters.AddWithValue("$id", id.ToString());
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        transaction.Commit();

        return payment with
        {
            Status = requestedStatus,
            SubscriptionId = subscriptionId,
            UpdatedAt = updatedAt
        };
    }

    public async Task<IReadOnlyList<SubscriptionRecord>> GetSubscriptionsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM subscriptions ORDER BY created_at DESC";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var subscriptions = new List<SubscriptionRecord>();
        while (await reader.ReadAsync(cancellationToken))
            subscriptions.Add(ReadSubscription(reader));
        return subscriptions;
    }

    public async Task<SubscriptionRecord?> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM subscriptions WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadSubscription(reader) : null;
    }

    private async Task<PaymentRecord?> GetPaymentInTransactionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT * FROM payments WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadPayment(reader) : null;
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static PaymentRecord ReadPayment(SqliteDataReader reader) => new(
        Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
        reader.GetString(reader.GetOrdinal("idempotency_key")),
        Enum.Parse<PaymentProvider>(reader.GetString(reader.GetOrdinal("provider"))),
        Enum.Parse<PaymentPurpose>(reader.GetString(reader.GetOrdinal("purpose"))),
        Enum.Parse<PaymentStatus>(reader.GetString(reader.GetOrdinal("status"))),
        reader.GetInt32(reader.GetOrdinal("amount")),
        reader.GetString(reader.GetOrdinal("currency")),
        reader.GetString(reader.GetOrdinal("phone_number")),
        reader.GetString(reader.GetOrdinal("customer_name")),
        ReadNullableDateOnly(reader, "birth_date"),
        ReadNullableString(reader, "address"),
        ReadNullableString(reader, "category_id"),
        ReadNullableString(reader, "category_name"),
        ReadNullableString(reader, "plan_name"),
        ReadNullableGuid(reader, "subscription_id"),
        DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("created_at")), CultureInfo.InvariantCulture),
        DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("updated_at")), CultureInfo.InvariantCulture));

    private static SubscriptionRecord ReadSubscription(SqliteDataReader reader) => new(
        Guid.Parse(reader.GetString(reader.GetOrdinal("id"))),
        Guid.Parse(reader.GetString(reader.GetOrdinal("initial_payment_id"))),
        reader.GetString(reader.GetOrdinal("customer_name")),
        reader.GetString(reader.GetOrdinal("phone_number")),
        DateOnly.Parse(reader.GetString(reader.GetOrdinal("birth_date")), CultureInfo.InvariantCulture),
        reader.GetString(reader.GetOrdinal("address")),
        reader.GetString(reader.GetOrdinal("category_id")),
        reader.GetString(reader.GetOrdinal("category_name")),
        reader.GetString(reader.GetOrdinal("plan_name")),
        reader.GetInt32(reader.GetOrdinal("monthly_amount")),
        DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("created_at")), CultureInfo.InvariantCulture));

    private static void AddPaymentParameters(SqliteCommand command, PaymentRecord payment)
    {
        command.Parameters.AddWithValue("$id", payment.Id.ToString());
        command.Parameters.AddWithValue("$key", payment.IdempotencyKey);
        command.Parameters.AddWithValue("$provider", payment.Provider.ToString());
        command.Parameters.AddWithValue("$purpose", payment.Purpose.ToString());
        command.Parameters.AddWithValue("$status", payment.Status.ToString());
        command.Parameters.AddWithValue("$amount", payment.Amount);
        command.Parameters.AddWithValue("$currency", payment.Currency);
        command.Parameters.AddWithValue("$phone", payment.PhoneNumber);
        command.Parameters.AddWithValue("$customer", payment.CustomerName);
        command.Parameters.AddWithValue("$birthDate", payment.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) is { } date ? date : DBNull.Value);
        command.Parameters.AddWithValue("$address", payment.Address is { } address ? address : DBNull.Value);
        command.Parameters.AddWithValue("$categoryId", payment.CategoryId is { } categoryId ? categoryId : DBNull.Value);
        command.Parameters.AddWithValue("$categoryName", payment.CategoryName is { } categoryName ? categoryName : DBNull.Value);
        command.Parameters.AddWithValue("$planName", payment.PlanName is { } planName ? planName : DBNull.Value);
        command.Parameters.AddWithValue("$created", payment.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$updated", payment.UpdatedAt.ToString("O", CultureInfo.InvariantCulture));
    }

    private static DateOnly? ReadNullableDateOnly(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal)
            ? null
            : DateOnly.Parse(reader.GetString(ordinal), CultureInfo.InvariantCulture);
    }

    private static string? ReadNullableString(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static Guid? ReadNullableGuid(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : Guid.Parse(reader.GetString(ordinal));
    }

    private static string BuildConnectionString(IConfiguration configuration, string contentRootPath)
    {
        var configured = configuration.GetConnectionString("Payments") ?? "Data Source=Data/microassur.db";
        var builder = new SqliteConnectionStringBuilder(configured);
        var databasePath = Path.GetFullPath(builder.DataSource, contentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        builder.DataSource = databasePath;
        return builder.ToString();
    }
}
