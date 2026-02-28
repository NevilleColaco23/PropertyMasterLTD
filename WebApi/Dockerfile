# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY *.sln .
COPY WebApi/*.csproj WebApi/
COPY Messaging.Shared/*.csproj Messaging.Shared/
COPY classfiles/Application/*.csproj classfiles/Application/
COPY classfiles/Infrastructure/*.csproj classfiles/Infrastructure/
COPY classfiles/Domain/*.csproj classfiles/Domain/
COPY classfiles/MongoDBBackend/*.csproj classfiles/MongoDBBackend/
COPY classfiles/SampleData/*.csproj classfiles/SampleData/

# Restore dependencies
RUN dotnet restore WebApi/WebApi.csproj

# Copy everything else and build
COPY . .

# Publish the WebApi project to the /app/publish directory
WORKDIR /src/WebApi
ARG CACHEBUST=2
RUN dotnet publish -c Release -o /app/publish --no-restore
RUN echo '=== FILE LISTING OF /app/publish BELOW ===' && ls -la /app/publish && echo '=== END OF FILE LISTING ==='

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Copy entrypoint script
COPY WebApi/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Railway provides PORT environment variable - we'll use it at runtime
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Logging__Console__FormatterName=Simple
ENV Logging__LogLevel__Default=Information
ENV Logging__LogLevel__Microsoft=Information
ENV Logging__LogLevel__Microsoft.Hosting.Lifetime=Information

# Expose the port Railway will route to (Railway sets $PORT dynamically)
EXPOSE 8080

# Start the API using entrypoint script
ENTRYPOINT ["/app/entrypoint.sh"]