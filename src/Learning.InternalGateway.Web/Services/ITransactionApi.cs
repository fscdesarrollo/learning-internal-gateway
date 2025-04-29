using Learning.InternalGateway.Shared;
using Microsoft.AspNetCore.Mvc;
using Refit;

public interface ITransactionApi
{
    [Post("/internal-gateway/transactions")]
    Task<ApiResponse<object>> CreateTransactionAsync([Body] TransactionRequest request,
        [Header("x-client-request-id")] string xClientRequestId);
}
