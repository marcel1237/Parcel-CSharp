#!/usr/bin/env bash
set -e

echo "=================================================="
echo "  Building Parcel C# Industrial Toolkit (.NET 8)"
echo "=================================================="

# Build solution
dotnet build ParcelCSharp.sln -c Release

echo "=================================================="
echo "  Build Completed Successfully!"
echo "=================================================="
