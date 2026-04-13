# JobQueue
 
A background job processor for SaaS applications. Accepts jobs, queues them using the transactional outbox pattern, processes them with priority awareness, retries on failure with exponential backoff, and surfaces everything in a React dashboard.
 
Built with .NET 10, PostgreSQL, RabbitMQ, and React.
 
---
 
## Usage
 
### Prerequisites
 
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/)
 
### 1. Start infrastructure
 
```bash
docker compose up
```
 
### 2. Apply migrations
 
```bash
cd JobQueue.Api
dotnet ef database update
```
 
### 3. Run the API
 
```bash
cd JobQueue.Api
dotnet run
# http://localhost:5258/api
```
 
### 4. Run the Worker
 
```bash
cd JobQueue.Worker
dotnet run
```
 
### 5. Run the Dashboard
 
```bash
cd JobQueue.Dashboard
npm install
npm run dev
# http://localhost:5173
```
 
---
 
## Creating a Job
 
```bash
curl -X POST http://localhost:5258/api/jobs \
  -H "Content-Type: application/json" \
  -d '{
    "type": "SendEmail",
    "priority": "Normal",
    "payload": "{\"email\":\"user@example.com\",\"subject\":\"Hello\",\"body\":\"World\"}"
  }'
```
 
Valid `type` values: `SendEmail`, `GeneratePdf`, `ProcessImage`, `DeliverWebhook`
 
Valid `priority` values: `Low`, `Normal`, `High`
