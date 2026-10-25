#!/usr/bin/env bash
set -e

echo "=================================================="
echo "  Running Parcel C# Desktop Application"
echo "=================================================="

dotnet run --project ParcelDesktop/ParcelDesktop.csproj -c Release
