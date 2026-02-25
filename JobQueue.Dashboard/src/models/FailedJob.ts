export type FailedJob = {
    id: number;
    type: string;
    createdAt: Date; 
    updatedAt: Date; 
    errorMessage: string;
}