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
RUN dotnet publish -c Release -o /app/publish --no-restore
ARG CACHEBUST=1
RUN echo '=== FILE LISTING OF /app/publish BELOW ===' && ls -la /app/publish && echo '=== END OF FILE LISTING ==='

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Set environment variables for better logging
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Logging__Console__FormatterName=Simple
ENV Logging__LogLevel__Default=Information
ENV Logging__LogLevel__Microsoft=Information
ENV Logging__LogLevel__Microsoft.Hosting.Lifetime=Information

EXPOSE 80

# Start the API with verbose output
ENTRYPOINT ["dotnet", "MyWarehouse.WebApi.dll"]