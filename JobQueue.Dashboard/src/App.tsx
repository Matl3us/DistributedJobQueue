import { useEffect, useState } from "react";
import type { StatusJobsCount } from "./models/StatusJobsCount.ts";
import { GetJobsCountByStatuses } from "./api/endpoints.ts";

function App() {
  const [jobsCountByStatus, setJobsCountByStatus] = useState<StatusJobsCount>();

  useEffect(
    () => {
      GetJobsCountByStatuses()
        .then((res) => setJobsCountByStatus(res));
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
    </div>
  )
}

export default App
