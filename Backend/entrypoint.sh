#!/bin/bash
set -e

echo "Waiting for db"
for i in {1..30}; do
    if pg_isready -h ${DB_HOST:-database} -U ${DB_USER:-postgres} -p ${DB_PORT:-5432} 2>/dev/null; then
        echo "db is ready"
        break
    fi
    echo "Attempt $i/30"
    sleep 1
done

cd /src
echo "Creating Entity Framework migrations"
dotnet ef migrations add InitialCreate --project Backend.Persistence --startup-project Backend.API --output-dir Migrations 2>/dev/null || echo "Migrations already exist"

echo "Applying Entity Framework migrations"
dotnet ef database update --project Backend.Persistence --startup-project Backend.API

echo "Migrations completed."
cd /app
echo "Starting Backend API"
exec dotnet Backend.API.dll
