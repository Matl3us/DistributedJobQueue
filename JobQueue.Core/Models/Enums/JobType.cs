namespace JobQueue.Core.Models.Enums;

public enum JobType
{
    SendEmail,
    GeneratePdf,
    ProcessImage,
    DeliverWebhook
}