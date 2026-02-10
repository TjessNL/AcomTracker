public class Payment
{

    public DateTime Date { get; set; }
    public required Tenant Name {get; set;}
    public required string Method { get; set; }
    public decimal Amount {get; set;}
    public int PaymentId {get; set;}
    
}
