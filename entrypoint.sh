#!/bin/bash

echo "⏳ Waiting for SQL Server..."
sleep 20

echo "🛠️ Running EF Core migrations..."
dotnet ef database update

echo "🚀 Starting the API..."
exec dotnet PatientApi.dll