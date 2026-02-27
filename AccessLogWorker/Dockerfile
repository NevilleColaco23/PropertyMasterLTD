FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AccessLogWorker/AccessLogConsumerWorker.csproj", "AccessLogWorker/"]
COPY ["Messaging.Shared/Messaging.Shared.csproj", "Messaging.Shared/"]
COPY ["classfiles/Domain/Domain.csproj", "classfiles/Domain/"]
COPY ["classfiles/Application/Application.csproj", "classfiles/Application/"]
COPY ["classfiles/Infrastructure/Infrastructure.csproj", "classfiles/Infrastructure/"]
COPY ["classfiles/MongoDBBackend/MongoDBBackend.csproj", "classfiles/MongoDBBackend/"]

# Restore dependencies
RUN dotnet restore "AccessLogWorker/AccessLogConsumerWorker.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/AccessLogWorker"
RUN dotnet publish "AccessLogConsumerWorker.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AccessLogConsumerWorker.dll"]