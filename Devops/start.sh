#!/bin/bash
# ============================================
# Transendence - WSL2 Startup Script
# Her WSL açılışında bu scripti çalıştır:
#   ./start.sh
# ============================================

set -e
cd "$(dirname "$0")"

echo "🚀 Transendence Başlatılıyor..."
echo "================================"

# 1. Docker çalışıyor mu kontrol et
echo ""
echo "🐳 [1/3] Docker kontrol ediliyor..."
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker çalışmıyor!"
    echo "   → Windows'ta Docker Desktop'ı aç ve WSL2 entegrasyonunu aktif et."
    echo "   → Docker Desktop ayaklandıktan sonra bu scripti tekrar çalıştır."
    exit 1
fi
echo "   ✅ Docker çalışıyor"

# 2. Servisleri başlat
echo ""
echo "📦 [2/3] Servisler başlatılıyor..."
docker compose up -d

# 3. Health check'leri bekle
echo ""
echo "⏳ [3/3] Servisler hazır olana kadar bekleniyor..."

MAX_WAIT=90
WAITED=0

while [ $WAITED -lt $MAX_WAIT ]; do
    # Tüm container'ların health durumunu kontrol et
    UNHEALTHY=$(docker ps --filter "name=transendence" --format "{{.Names}} {{.Status}}" | grep -c "starting\|unhealthy" || true)
    
    if [ "$UNHEALTHY" -eq 0 ]; then
        break
    fi
    
    echo "   ⏳ $UNHEALTHY servis hâlâ hazırlanıyor... (${WAITED}s)"
    sleep 5
    WAITED=$((WAITED + 5))
done

echo ""
echo "================================"

if [ $WAITED -ge $MAX_WAIT ]; then
    echo "⚠️  Bazı servisler hâlâ hazır değil, ama erişmeyi deneyebilirsin."
else
    echo "✅ Tüm servisler hazır!"
fi

echo ""
echo "🌐 Windows tarayıcından erişim:"
echo "   Frontend:  https://localhost"
echo "   Backend:   https://localhost/api"
echo "   Grafana:   https://localhost/grafana/"
echo "   Prometheus: http://localhost:9090"
echo ""
echo "📝 Faydalı komutlar:"
echo "   make logs     → Tüm logları gör"
echo "   make down     → Servisleri durdur"
echo "   make rebuild  → Sıfırdan başlat"
echo "================================"
