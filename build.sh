#!/usr/bin/env bash
set -euo pipefail
dotnet run --project tools/Build/Build.csproj -- "$@"
