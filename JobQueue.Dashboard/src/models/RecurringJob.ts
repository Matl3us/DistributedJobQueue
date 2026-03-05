export type RecurringJob = {
    id: number;
    name: string;
    type: string;
    cronExpression: string;
    lastRun: Date | null;
    nextRun: Date | null;
}