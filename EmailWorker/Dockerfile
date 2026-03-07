# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["EmailWorker/EmailWorker.csproj", "EmailWorker/"]
COPY ["Messaging.Shared/Messaging.Shared.csproj", "Messaging.Shared/"]
RUN dotnet restore "EmailWorker/EmailWorker.csproj"

# Copy everything else and build
COPY EmailWorker/ EmailWorker/
COPY Messaging.Shared/ Messaging.Shared/
WORKDIR "/src/EmailWorker"
RUN dotnet build "EmailWorker.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EmailWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD pgrep -f EmailWorker || exit 1

ENTRYPOINT ["dotnet", "EmailWorker.dll"]
