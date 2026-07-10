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

# 1.5. Elasticsearch için bellek ayarını kontrol et
echo ""
echo "⚙️ [1.5/3] Elasticsearch sistem ayarları kontrol ediliyor..."
CURRENT_MAP_COUNT=$(sysctl -n vm.max_map_count)
if [ "$CURRENT_MAP_COUNT" -lt 262144 ]; then
    echo "⚠️ vm.max_map_count değeri çok düşük ($CURRENT_MAP_COUNT)."
    echo "Elasticsearch'ün çalışabilmesi için bu değerin en az 262144 olması gerekiyor."
    echo "Değeri şimdi güncelliyorum (Şifre sorulabilir)..."
    sudo sysctl -w vm.max_map_count=262144
fi
echo "   ✅ Sistem ayarları uygun"

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
