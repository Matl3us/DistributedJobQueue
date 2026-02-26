import { useEffect, useState } from "react";
import type { StatusJobsCount } from "./models/StatusJobsCount.ts";
import {
  GetDeadLetterQueueJobsPaginated,
  GetFailedJobsPaginated,
  GetJobsCountByStatuses,
  RetryJob
} from "./api/endpoints.ts";
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
    }, []);

  const retryCallGenerator = (id: number) => {
    return async () => RetryJob(id);
  }

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
          <div className="p-2 max-w-[550px] max-h-[600px] flex flex-col gap-4 overflow-y-scroll">
            {failedJobs.map(f => (
              <div key={f.id} className="p-6 bg-zinc-800 rounded-lg">
                <div className="flex">
                  <div >
                    <p>Id: {f.id}</p>
                    <p>Type: {f.type}</p>
                    <p>Created at: {f.createdAt.toString()}</p>
                    <p>Updated at: {f.updatedAt.toString()}</p>
                    <p>Error messages: {f.errorMessage}</p>
                  </div>
                  <div>
                    <button className="p-2 rounded-lg bg-blue-600 hover:bg-blue-800" onClick={retryCallGenerator(f.id)}>
                      <p>Retry</p>
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
        <div className="p-4 bg-zinc-900 rounded-lg">
          <p className="text-2xl p-2">Dead letter queue jobs</p>
          <div className="max-w-[550px] max-h-[600px] overflow-y-scroll">
            {deadLetterQueueJobs.map(d => (
              <div key={d.id} className="p-2">
                <div className="p-4 bg-zinc-800 rounded-lg">
                  <p>Id: {d.id}</p>
                  <p>Type: {d.type}</p>
                  <p>Created at: {d.createdAt.toString()}</p>
                  <p>Updated at: {d.updatedAt.toString()}</p>
                  <p>Error messages: {d.errorMessage}</p>
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
