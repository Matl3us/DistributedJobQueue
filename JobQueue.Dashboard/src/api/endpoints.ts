import type { StatusJobsCount } from "../models/StatusJobsCount.ts";
import { getApi, postApi } from "./api.ts";
import type { FailedJob } from "../models/FailedJob.ts";
import type { RecurringJob } from "../models/RecurringJob.ts";

export async function GetJobsCountByStatuses(): Promise<StatusJobsCount> {
    return await getApi<StatusJobsCount>("jobs/status/count");
}

export async function GetFailedJobsPaginated(): Promise<Array<FailedJob>> {
    return await getApi<Array<FailedJob>>("jobs/failed");
}

export async function GetDeadLetterQueueJobsPaginated(): Promise<Array<FailedJob>> {
    return await getApi<Array<FailedJob>>("jobs/deadLetterQueue");
}

export async function GetRecurringJobsPaginated(): Promise<Array<RecurringJob>> {
    return await getApi<Array<RecurringJob>>("recurring-jobs/all");
}

export async function RetryJob(id: number): Promise<null> {
    return await postApi<null>(`jobs/${id}/retry`, null);
}