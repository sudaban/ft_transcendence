FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src

COPY ["Backend.csproj", "./"]

RUN dotnet restore "Backend.csproj"

COPY . .

RUN dotnet build "Backend.csproj" -c Release -o /app/build

RUN dotnet publish "Backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Backend.dll"]
