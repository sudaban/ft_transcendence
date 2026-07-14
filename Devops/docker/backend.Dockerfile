FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

WORKDIR /src

COPY Backend.Domain/Backend.Domain.csproj Backend.Domain/

COPY Backend.Application/Backend.Application.csproj Backend.Application/

COPY Backend.Infrastructure/Backend.Infrastructure.csproj Backend.Infrastructure/

COPY Backend.Persistence/Backend.Persistence.csproj Backend.Persistence/

COPY Backend.API/Backend.API.csproj Backend.API/

RUN dotnet restore Backend.API/Backend.API.csproj

COPY . .

RUN dotnet publish Backend.API/Backend.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=builder /app/publish .

COPY entrypoint.sh .

RUN chmod +x entrypoint.sh && apk add --no-cache postgresql-client

EXPOSE 5000

ENTRYPOINT ["/app/entrypoint.sh"]
