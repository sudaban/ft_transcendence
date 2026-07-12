# ============================================================
# Nginx Dockerfile — Reverse Proxy & SSL Terminasyonu
# ============================================================
# Bu Dockerfile, Nginx reverse proxy sunucusunu kurar.
# Görevleri:
#   1. Self-signed SSL sertifikası oluşturur (HTTPS için)
#   2. Frontend, Backend ve Grafana'ya gelen istekleri yönlendirir
#   3. HTTP → HTTPS yönlendirmesi yapar
#   4. WebSocket bağlantılarını destekler
# ============================================================

# Alpine tabanlı hafif Nginx image'ı (~5MB)
FROM nginx:alpine

# SSL sertifikası oluşturmak için OpenSSL'i yükle
RUN apk add --no-cache openssl

# Self-signed SSL sertifikası oluştur (geliştirme ortamı için)
# Tarayıcıda "güvenli değil" uyarısı verir — production'da gerçek sertifika kullanılmalı
# -x509: Self-signed sertifika formatı
# -nodes: Şifresiz private key (container yeniden başlatıldığında şifre sormasın)
# -days 365: 1 yıl geçerli
# -newkey rsa:2048: 2048 bit RSA anahtarı oluştur
RUN mkdir -p /etc/nginx/ssl && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/nginx/ssl/nginx.key \
    -out /etc/nginx/ssl/nginx.crt \
    -subj "/C=TR/ST=Kocaeli/L=Kocaeli/O=42/CN=localhost"

# Nginx yapılandırma dosyasını kopyala
# nginx.conf: Proxy kuralları, SSL ayarları, upstream tanımları
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 80: HTTP (HTTPS'e yönlendirilir)
# 443: HTTPS (asıl trafik)
EXPOSE 80 443

# Nginx'i foreground modda çalıştır (Docker container olarak çalışması için)
CMD ["nginx", "-g", "daemon off;"]
