#!/bin/bash
set -e

echo "=== Railway Container Starting ==="
echo "PORT: ${PORT:-8080}"
echo "ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT}"
echo "Working Directory: $(pwd)"
echo "Files in directory:"
ls -la
echo "=================================="

# Set the ASPNETCORE_URLS using Railway's PORT variable
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"

echo "Starting WebApi on ${ASPNETCORE_URLS}"

# Start the application
exec dotnet MyWarehouse.WebApi.dll
