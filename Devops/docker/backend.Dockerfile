# ============================================================
# Backend Dockerfile — .NET API Uygulaması
# ============================================================
# Bu Dockerfile, .NET Backend API'sini iki aşamalı (multi-stage)
# olarak derler. İlk aşamada SDK ile kaynak kod derlenir,
# ikinci aşamada sadece çalıştırılabilir dosyalar hafif bir
# runtime image'ına kopyalanır.
# Sonuç image boyutu: ~100MB (SDK kullanılsaydı ~800MB olurdu)
# ============================================================

# ---- Aşama 1: Build (Derleme) ----
# SDK image'ı sadece derleme için kullanılır, final image'a dahil edilmez
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder

WORKDIR /src

# Önce sadece .csproj dosyalarını kopyalayıp restore yapıyoruz
# Bu sayede kaynak kod değişmediği sürece NuGet paketleri cache'den gelir
# ve build süresi önemli ölçüde kısalır (Docker layer caching)
COPY Backend.Domain/Backend.Domain.csproj Backend.Domain/
COPY Backend.Application/Backend.Application.csproj Backend.Application/
COPY Backend.Infrastructure/Backend.Infrastructure.csproj Backend.Infrastructure/
COPY Backend.Persistence/Backend.Persistence.csproj Backend.Persistence/
COPY Backend.API/Backend.API.csproj Backend.API/
RUN dotnet restore Backend.API/Backend.API.csproj

# Tüm kaynak kodu kopyala ve Release modunda derle
COPY . .
RUN dotnet publish Backend.API/Backend.API.csproj -c Release -o /app/publish

# ---- Aşama 2: Runtime (Çalıştırma) ----
# aspnet image'ı SDK'nın ~8 katı daha küçüktür (~100MB vs ~800MB)
# Sadece .NET uygulamasını çalıştırmak için gerekli bileşenleri içerir
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

# Derlenen uygulama dosyalarını builder aşamasından kopyala
COPY --from=builder /app/publish .

# entrypoint.sh: Veritabanının hazır olmasını bekleyip uygulamayı başlatan script
COPY entrypoint.sh .
# postgresql-client: entrypoint.sh'deki pg_isready komutu için gerekli
RUN chmod +x entrypoint.sh && apk add --no-cache postgresql-client

# Backend API 5000 portunda çalışır
EXPOSE 5000

# Container başlatıldığında entrypoint.sh çalıştırılır
# entrypoint.sh → DB'yi bekle → dotnet Backend.API.dll
ENTRYPOINT ["/app/entrypoint.sh"]
