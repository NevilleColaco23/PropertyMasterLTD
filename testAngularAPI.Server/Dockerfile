# --- Stage 1: Build Angular App ---
FROM node:18 AS angular-builder

WORKDIR /app

# Copy Angular project files
COPY app/package*.json ./
RUN npm install --legacy-peer-deps

COPY app/ ./
RUN npm run build -- --configuration production


# --- Stage 2: Build .NET API ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-builder

WORKDIR /src

# Copy API source and solution file
COPY testAngularAPI.Server/ ./testAngularAPI.Server/
COPY testAngularAPI.sln ./

RUN dotnet restore testAngularAPI.Server/testAngularAPI.Server.csproj
RUN dotnet publish testAngularAPI.Server/testAngularAPI.Server.csproj -c Release -o /app/publish

# --- Stage 3: Final image to run app ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy .NET publish output
COPY --from=dotnet-builder /app/publish .

# Copy Angular build output into wwwroot of .NET app
COPY --from=angular-builder /app/dist/testangularapi.client/browser/ /app/wwwroot

ENV ASPNETCORE_URLS=http://+:${PORT:-80}

EXPOSE 80

ENTRYPOINT ["dotnet", "testAngularAPI.Server.dll"]
