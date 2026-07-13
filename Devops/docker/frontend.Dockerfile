# ============================================================
# Frontend Dockerfile — SvelteKit Uygulaması
# ============================================================
# Bu Dockerfile, SvelteKit frontend uygulamasını iki aşamalı
# (multi-stage) olarak derler. İlk aşamada npm ile bağımlılıklar
# yüklenip uygulama derlenir, ikinci aşamada sadece derlenen
# çıktı (build/) ve production bağımlılıkları kopyalanır.
# Bu sayede devDependencies ve kaynak kodlar final image'da yer almaz.
# ============================================================

# ---- Aşama 1: Build (Derleme) ----
FROM node:20-alpine AS builder

WORKDIR /app

# Önce package dosyalarını kopyalayıp bağımlılıkları yükle
# Bu sayede kaynak kod değişse bile node_modules cache'den gelir
COPY package.json ./
# --prefer-offline: Mümkünse cache'den yükle (hız)
# --no-audit: Güvenlik denetimini atla (hız)
# --no-fund: Bağış mesajlarını gösterme (temiz çıktı)
# --maxsockets=5: Eşzamanlı bağlantı sayısını sınırla (kararlılık)
RUN npm install --prefer-offline --no-audit --no-fund --maxsockets=5

# Kaynak kodu kopyala, uygulamayı derle ve dev bağımlılıkları temizle
COPY . .
# npm run build: SvelteKit uygulamasını production için derler → build/ klasörü
# npm prune --production: devDependencies'i siler (test, lint vs. paketleri)
RUN npm run build && npm prune --production

# ---- Aşama 2: Production (Çalıştırma) ----
# Temiz bir Node.js image'ı — sadece çalıştırma için gerekli dosyalar
FROM node:20-alpine

WORKDIR /app

# Sadece derlenmiş çıktıyı, package.json'ı ve production bağımlılıkları kopyala
# Kaynak kod (.svelte, .ts dosyaları) ve devDependencies dahil EDİLMEZ
COPY --from=builder /app/build ./build
COPY --from=builder /app/package.json ./
COPY --from=builder /app/node_modules ./node_modules

# SvelteKit Node adapter varsayılan olarak 3000 portunu kullanır
EXPOSE 3000

# Derlenmiş uygulamayı Node.js ile başlat
CMD ["node", "build/index.js"]
