import type { StatusJobsCount } from "../models/StatusJobsCount.ts";
import { api } from "./api.ts";
import type {FailedJob} from "../models/FailedJob.ts";

export async function GetJobsCountByStatuses(): Promise<StatusJobsCount> {
   return await api<StatusJobsCount>("jobs/status/count");
}

export async function GetFailedJobsPaginated(): Promise<Array<FailedJob>>
{
   return await api<Array<FailedJob>>("jobs/failed");
}

export async function GetDeadLetterQueueJobsPaginated(): Promise<Array<FailedJob>>
{
    return await api<Array<FailedJob>>("jobs/deadLetterQueue");
}