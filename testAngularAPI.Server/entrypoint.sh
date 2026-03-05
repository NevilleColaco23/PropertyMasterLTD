#!/bin/bash
set -e

echo "=== Railway Container Starting ==="
echo "PORT: ${PORT:-8080}"
echo "ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}"
echo "Working Directory: $(pwd)"
echo "=================================="

# Set the ASPNETCORE_URLS using Railway's PORT variable
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"

echo "Starting testAngularAPI.Server on ${ASPNETCORE_URLS}"
echo "Executing: dotnet testAngularAPI.Server.dll"
echo "=================================="

# Start the application and show all output
dotnet testAngularAPI.Server.dll 2>&1 || {
    exitcode=$?
    echo "=================================="
    echo "ERROR: Application failed to start!"
    echo "Exit code: $exitcode"
    echo "=================================="
    exit $exitcode
}
