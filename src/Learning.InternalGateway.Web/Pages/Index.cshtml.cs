using Elastic.Apm;
using Learning.InternalGateway.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using System.Text.Json;

public class IndexModel : PageModel
{
    private readonly ITransactionApi _transactionApi;

    public IndexModel(ITransactionApi transactionApi)
    {
        _transactionApi = transactionApi;
    }

    [BindProperty]
    public TransactionRequest Transaction { get; set; }

    public string ResponseJson { get; set; }
    public string ClientRequestId { get; set; }
    public string TraceId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var trx = Agent.Tracer.CurrentTransaction ?? Agent.Tracer.StartTransaction("web-request", "request");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            ClientRequestId = Guid.NewGuid().ToString();
            TraceId = trx.TraceId.ToString();

            var response = await _transactionApi.CreateTransactionAsync(Transaction, ClientRequestId);
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            if (response.IsSuccessStatusCode)
            {
                ResponseJson = JsonSerializer.Serialize(response.Content, jsonOptions);
            }
            else
            {
                ResponseJson = JsonSerializer.Serialize(new
                {
                    Error = true,
                    StatusCode = response.StatusCode,
                    Reason = response.Error?.ToString()
                }, jsonOptions);
            }
        }
        catch (ApiException ex)
        {
            ResponseJson = JsonSerializer.Serialize(new
            {
                Error = true,
                StatusCode = ex.StatusCode,
                Content = ex.Content
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        return Page();
    }
}
