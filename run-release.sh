#!/usr/bin/env bash
set -e

echo "=================================================="
echo "  Running Parcel C# from Release Build"
echo "=================================================="

RELEASE_DLL="ParcelDesktop/bin/Release/net8.0/ParcelDesktop.dll"
RELEASE_EXE="ParcelDesktop/bin/Release/net8.0/ParcelDesktop"

if [ -f "$RELEASE_EXE" ]; then
    "$RELEASE_EXE"
elif [ -f "$RELEASE_DLL" ]; then
    dotnet "$RELEASE_DLL"
else
    echo "Release build not found! Running build.sh first..."
    ./build.sh
    dotnet "$RELEASE_DLL"
fi
