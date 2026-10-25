using System.Text.Json.Serialization;
using MicroAssur.Api.Endpoints;
using MicroAssur.Api.Middleware;
using MicroAssur.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidation();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton<IPaymentStore, SqlitePaymentStore>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IPaymentAggregator, SimulatedPaymentAggregator>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

await app.Services.GetRequiredService<IPaymentStore>().InitializeAsync();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithSummary("Vérifie que l’API fonctionne.")
    .Produces(StatusCodes.Status200OK);

app.MapPaymentEndpoints();
app.MapSubscriptionEndpoints();

app.Run();

public partial class Program;
