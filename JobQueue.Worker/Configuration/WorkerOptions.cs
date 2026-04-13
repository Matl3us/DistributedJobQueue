namespace JobQueue.Worker.Configuration;

public class WorkerOptions
{
    public const string Worker = "Worker";

    public int MaxJobRetries { get; init; }
    public ushort PrefetchCount { get; init; }
}