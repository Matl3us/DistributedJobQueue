FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobQueue.Worker/JobQueue.Worker.csproj", "JobQueue.Worker/"]
COPY ["JobQueue.Infrastructure/JobQueue.Infrastructure.csproj", "JobQueue.Infrastructure/"]
COPY ["JobQueue.Core/JobQueue.Core.csproj", "JobQueue.Core/"]
RUN dotnet restore "JobQueue.Worker/JobQueue.Worker.csproj"
COPY . .
WORKDIR "/src/JobQueue.Worker"
RUN dotnet build "./JobQueue.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JobQueue.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobQueue.Worker.dll"]
