using Elastic.Apm.NetCoreAll;
using Learning.InternalGateway.Shared;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


var elasticApmServerUrl = builder.Configuration.GetValue<string>("ElasticApm:ServerUrl");
var elasticServiceName = builder.Configuration.GetValue<string>("ElasticApm:ServiceName") ?? "InternalGateway";

builder.Services.ConfigureLogger(builder.Environment,
    builder.Configuration,
    elasticServiceName);

builder.Host.UseSerilog();

var app = builder.Build();

app.UseAllElasticApm(builder.Configuration);
app.UseHttpsRedirection();

var summaries = new[]
{
       "Credit", "Debit"
    };

app.MapGet("/api/v1/transactions", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
            new Transactions
            (
                DateTime.Now,
                Random.Shared.Next(1, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
    return forecast;
});

app.MapPost("/api/v1/transactions", ([FromBody] TransactionRequest request, 
    [FromHeader(Name = "x-client-request-id")] string xClientRequestId) =>
{
    return Results.Ok(new Transactions
    (
        DateTime.Now,
        request.Amount,
        request.TransactionType
    ));
});

app.Run();

internal record Transactions(DateTime Date, decimal Amount, string? TransactionType)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public decimal Amount { get; init; } = Amount;
}
