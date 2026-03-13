namespace OcppEvCharging.Server.Models.Enums;

public static class ReadingContext
{
    public const string InterruptionBegin = "Interruption.Begin";
    public const string InterruptionEnd = "Interruption.End";
    public const string SampleClock = "Sample.Clock";
    public const string SamplePeriodic = "Sample.Periodic";
    public const string TransactionBegin = "Transaction.Begin";
    public const string TransactionEnd = "Transaction.End";
    public const string Trigger = "Trigger";
    public const string Other = "Other";
}
