import type { StatusJobsCount } from "../models/StatusJobsCount.ts";
import { api } from "./api.ts";

export async function GetJobsCountByStatuses(): Promise<StatusJobsCount> {
   return await api<StatusJobsCount>("jobs/status/count");
}