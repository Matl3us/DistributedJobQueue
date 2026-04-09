using JobQueue.Core.Models.Enums;

namespace JobQueue.Core.Models.DTOs.JobPayloads;

public record ProcessImagePayload
{
    public required string SourcePath { get; init; }
    public ImageOperation Operation { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public int? Quality { get; init; }
    public string? WatermarkText { get; init; }
}