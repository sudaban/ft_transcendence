FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src

COPY Backend.csproj .
RUN dotnet restore Backend.csproj

COPY . .
RUN dotnet publish Backend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine

WORKDIR /app

COPY --from=builder /app/publish .
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="/root/.dotnet/tools:${PATH}"

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh && apk add --no-cache postgresql-client

EXPOSE 5000

ENTRYPOINT ["/app/entrypoint.sh"]
