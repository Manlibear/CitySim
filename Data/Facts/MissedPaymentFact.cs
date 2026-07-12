namespace CitySim.Data.Facts;

public class MissedPaymentFact(decimal amount) : IFact
{
    public decimal Amount { get; set; } = amount;
}
