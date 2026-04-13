namespace JobQueue.Core.Models;

public record JobResult
{
    public bool IsSuccess { get; init; }
    public string? Payload { get; init; }
    public string? ErrorMessage { get; init; }

    private JobResult() { }

    public static JobResult Success(string? payload = null)
    {
        return new JobResult
        {
            IsSuccess = true,
            Payload = payload
        };
    }

    public static JobResult Failure(string errorMessage)
    {
        return new JobResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}