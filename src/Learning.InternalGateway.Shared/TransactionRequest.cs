namespace Learning.InternalGateway.Shared;

public class TransactionRequest
{
    public string AccountNumber { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; }
}
