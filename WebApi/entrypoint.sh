#!/bin/bash
set -e

echo "=== PropertyMaster API Container Starting ==="
echo "PORT: ${PORT:-8080}"
echo "ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}"
echo "Working Directory: $(pwd)"
echo "============================================="

# Use PORT env var if set (Azure Container Apps injects this), default to 8080
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"

echo "Starting WebApi on ${ASPNETCORE_URLS}"
echo "Executing: dotnet MyWarehouse.WebApi.dll"
echo "============================================="

# Start the application and show all output
dotnet MyWarehouse.WebApi.dll 2>&1 || {
    exitcode=$?
    echo "============================================="
    echo "ERROR: Application failed to start!"
    echo "Exit code: $exitcode"
    echo "============================================="
    exit $exitcode
}
