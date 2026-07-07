#!/bin/sh
set -e

for i in $(seq 1 30); do
    if pg_isready -h "${DB_HOST:-database}" -U "${DB_USER:-postgres}" -p "${DB_PORT:-5432}" 2>/dev/null; then
        break
    fi
    sleep 1
done

cd /app
exec dotnet Backend.API.dll
