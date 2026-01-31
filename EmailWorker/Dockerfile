# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything 
COPY . .

# Publish the worker
RUN dotnet publish EmailWorker/EmailWorker.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build /app/publish .

# Start the worker
ENTRYPOINT ["dotnet", "EmailWorker.dll"]