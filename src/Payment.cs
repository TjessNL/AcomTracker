public class Payment
{

    public DateTime Date { get; set; }
    required public Tenant Name {get; set;}
    required public string Method { get; set; }
    public float Amount {get; set;}

    
}
