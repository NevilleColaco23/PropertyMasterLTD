# --- Stage 1: Build Angular App ---
FROM node:18 AS angular-builder

WORKDIR /app

COPY app/package*.json ./
RUN npm install --legacy-peer-deps

COPY app/ ./
RUN npm run build -- --configuration production


# --- Stage 2: Build .NET API ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-builder

WORKDIR /src

COPY testAngularAPI.sln ./
COPY testAngularAPI.Server/ ./testAngularAPI.Server/


# Copy all referenced projects
COPY classfiles/Domain/ ./classfiles/Domain/
COPY classfiles/MongoDBBackend/ ./classfiles/MongoDBBackend/
COPY classfiles/SampleData/ ./classfiles/SampleData/
COPY classfiles/Application/ ./classfiles/Application/
COPY classfiles/Infrastructure/ ./classfiles/Infrastructure/


RUN dotnet restore testAngularAPI.Server/testAngularAPI.Server.csproj
RUN dotnet publish testAngularAPI.Server/testAngularAPI.Server.csproj -c Release -o /app/publish


# --- Stage 3: Final runtime image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy backend publish output
COPY --from=dotnet-builder /app/publish ./

# Copy Angular build output
COPY --from=angular-builder /app/dist/testangularapi.client/browser/ ./wwwroot/

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "testAngularAPI.Server.dll"]
