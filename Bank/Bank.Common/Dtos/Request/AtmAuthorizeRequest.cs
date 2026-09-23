namespace Bank.Common.Dtos.Request;

public sealed class AtmAuthorizeRequest
{
    public string CardNumber { get; set; } = "";
    public string Pin { get; set; } = "";
}
