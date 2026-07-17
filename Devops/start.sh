#!/bin/bash
# ============================================
# transcendence - WSL2 Startup Script
# Her WSL açılışında bu scripti çalıştır:
#   ./start.sh
# ============================================

set -e
cd "$(dirname "$0")/.."

echo "🚀 transcendence Başlatılıyor..."
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
CURRENT_MAP_COUNT=$(sysctl -n vm.max_map_count)
if [ "$CURRENT_MAP_COUNT" -lt 262144 ]; then
    echo "⚠️ vm.max_map_count değeri düşük ($CURRENT_MAP_COUNT)."
    if sudo -n true 2>/dev/null; then
        echo "Değeri şimdi güncelliyorum..."
        sudo sysctl -w vm.max_map_count=262144
    else
        echo "Sudo yetkisi yok, atlanıyor."
        echo "Elasticsearch, node.store.allow_mmap=false ayarıyla mmap olmadan çalışacak şekilde yapılandırıldı."
    fi
fi

# 2. Servisleri başlat
echo ""
echo "📦 [2/3] Servisler başlatılıyor..."
docker compose up -d
# (Monitoring ve ELK için 'make full-up' kullanabilirsin)

# 3. Health check'leri bekle
echo ""
echo "⏳ [3/3] Servisler hazır olana kadar bekleniyor..."

MAX_WAIT=40
WAITED=0

while [ $WAITED -lt $MAX_WAIT ]; do
    # Tüm container'ların health durumunu kontrol et
    UNHEALTHY=$(docker ps --filter "name=transcendence" --format "{{.Names}} {{.Status}}" | grep -c "starting\|unhealthy" || true)
    
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
echo "   Frontend:   https://localhost:8443"
echo "   Backend API: https://localhost:8443/swagger"
echo "   Grafana:    https://localhost:8443/grafana/"
echo "   Kibana:     https://localhost:8443/kibana/"
echo "   Prometheus: https://localhost:8443/prometheus/"
echo ""
echo "📝 Faydalı komutlar:"
echo "   make full-up  → Tüm sistemi (Monitoring+ELK) başlat"
echo "   make full-down → Tüm servisleri durdur"
echo "   make nuke     → Tüm servisleri sıfırdan oluştur"
echo "================================"
