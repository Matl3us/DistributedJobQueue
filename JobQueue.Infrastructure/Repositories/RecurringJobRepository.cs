using Cronos;
using JobQueue.Core.Interfaces.Repositories;
using JobQueue.Core.Models.DTOs;
using JobQueue.Core.Models.Entities;
using JobQueue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace JobQueue.Infrastructure.Repositories;

public class RecurringJobRepository(JobContext context) : IRecurringJobRepository
{
    public RecurringJob Add(RecurringJobCreate recurringJobCreate)
    {
        var recurringJob = new RecurringJob
        {
            Name = recurringJobCreate.Name,
            Type = recurringJobCreate.Type,
            Payload = recurringJobCreate.Payload,
            CronExpression = recurringJobCreate.CronExpression,
            NextRun = recurringJobCreate.NextRun
        };

        var result = context.Add(recurringJob);
        return result.Entity;
    }

    public async Task<IEnumerable<RecurringJob>> GetPaginated(int page, int pageSize)
    {
        return await context.RecurringJobs.Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<RecurringJob?> GetDueAndLock()
    {
        var now = new NpgsqlParameter("now", DateTime.UtcNow);
        return await context.RecurringJobs
            .FromSql(
                $$$"""
                       SELECT * FROM public."RecurringJobs" 
                       WHERE "NextRun" <= {{{now}}} 
                       FOR UPDATE SKIP LOCKED LIMIT 1
                   """)
            .FirstOrDefaultAsync();
    }

    public void UpdateNextRun(RecurringJob recurringJob)
    {
        var nextRun = CronExpression
            .Parse(recurringJob.CronExpression)
            .GetNextOccurrence(DateTime.UtcNow);

        recurringJob.LastRun = recurringJob.NextRun;
        recurringJob.NextRun = nextRun;
    }

    public void Remove(RecurringJob recurringJob)
    {
        context.RecurringJobs.Remove(recurringJob);
    }
}