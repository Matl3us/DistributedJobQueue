import { useEffect, useState } from "react";
import type { StatusJobsCount } from "./models/StatusJobsCount.ts";
import { GetDeadLetterQueueJobsPaginated, GetFailedJobsPaginated, GetJobsCountByStatuses } from "./api/endpoints.ts";
import type { FailedJob } from "./models/FailedJob.ts";

function App() {
  const [jobsCountByStatus, setJobsCountByStatus] = useState<StatusJobsCount>();
  const [failedJobs, setFailedJobs] = useState<Array<FailedJob>>([]);
  const [deadLetterQueueJobs, setDeadLetterQueueJobs] = useState<Array<FailedJob>>([]);

  useEffect(
    () => {
      GetJobsCountByStatuses()
        .then((res) => setJobsCountByStatus(res));
      GetFailedJobsPaginated()
        .then((res) => setFailedJobs(res));
      GetDeadLetterQueueJobsPaginated()
        .then((res) => setDeadLetterQueueJobs(res));
    }
    , []);

  return (
    <div className="p-8">
      <h1>Distributed Job Queue System</h1>
      <h2>Dashboard</h2>
      <div className="py-6 flex gap-12 text-lg">
        <p className="text-yellow-400">Pending Jobs: {jobsCountByStatus?.pendingJobsCount}</p>
        <p className="text-orange-400">Processing Jobs: {jobsCountByStatus?.processingJobsCount}</p>
        <p className="text-green-400">Completed Jobs: {jobsCountByStatus?.completedJobsCount}</p>
        <p className="text-red-500">Failed Jobs: {jobsCountByStatus?.failedJobsCount}</p>
      </div>
      <div className="flex mt-6 gap-6">
        <div className="p-4 bg-zinc-900 rounded-lg">
          <p className="text-2xl p-2">Failed Jobs</p>
          <div className="max-w-[550px] max-h-[600px] overflow-y-scroll">
            {failedJobs.map(f => (
              <div className="p-2">
                <div className="p-4 bg-zinc-800 rounded-lg">
                  <p>Id: {f.id}</p>
                  <p>Type: {f.type}</p>
                  <p>Created at: {f.createdAt.toString()}</p>
                  <p>Updated at: {f.updatedAt.toString()}</p>
                  <p>Error messages: {f.errorMessage}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
          <div className="p-4 bg-zinc-900 rounded-lg">
          <p className="text-2xl p-2">Dead letter queue jobs</p>
          <div className="max-w-[550px] max-h-[600px] overflow-y-scroll">
              {deadLetterQueueJobs.map(f => (
                  <div className="p-2">
                      <div className="p-4 bg-zinc-800 rounded-lg">
                          <p>Id: {f.id}</p>
                          <p>Type: {f.type}</p>
                          <p>Created at: {f.createdAt.toString()}</p>
                          <p>Updated at: {f.updatedAt.toString()}</p>
                          <p>Error messages: {f.errorMessage}</p>
                      </div>
                  </div>
              ))}
          </div>
      </div>
      </div>
    </div>
  )
}

export default App
