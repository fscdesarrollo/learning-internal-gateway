using Elastic.Apm.NetCoreAll;
using Learning.InternalGateway.Shared;
using Refit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("TokenClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5172");
});

builder.Services.AddTransient<AuthorizationHandler>();

builder.Services.AddRefitClient<ITransactionApi>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5172");
    })
    .AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddRazorPages();

var elasticApmServerUrl = builder.Configuration.GetValue<string>("ElasticApm:ServerUrl");
var elasticServiceName = builder.Configuration.GetValue<string>("ElasticApm:ServiceName") ?? "InternalGateway";

builder.Services.ConfigureLogger(builder.Environment,
    builder.Configuration,
    elasticServiceName);

builder.Host.UseSerilog();

var app = builder.Build();
app.UseAllElasticApm(builder.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
