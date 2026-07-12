#!/bin/sh
# ============================================================
# Autoheal Script — Sağlıksız Container'ları Otomatik Yeniden Başlat
# ============================================================
# Bu script, Docker container'larının sağlık durumunu (healthcheck)
# düzenli aralıklarla kontrol eder. Eğer bir container "unhealthy"
# durumuna düşerse, otomatik olarak yeniden başlatır.
#
# Sadece "autoheal=true" label'ına sahip container'lar izlenir.
# docker-compose.yml'de labels: ["autoheal=true"] eklenmiş servisler.
#
# Ortam değişkenleri:
#   AUTOHEAL_INTERVAL     → Kontrol aralığı (saniye, varsayılan: 30)
#   AUTOHEAL_START_PERIOD → İlk bekleme süresi (saniye, varsayılan: 60)
# ============================================================

# Kontrol aralığı — kaç saniyede bir container sağlığı kontrol edilsin
INTERVAL=${AUTOHEAL_INTERVAL:-30}
# İlk bekleme süresi — container'ların başlaması için bekle
START_PERIOD=${AUTOHEAL_START_PERIOD:-60}

echo "Autoheal starting... waiting ${START_PERIOD}s for containers to initialize."
# Container'ların healthcheck'lerini geçmesi için başlangıçta bekle
sleep "$START_PERIOD"
echo "Autoheal active. Checking every ${INTERVAL}s for unhealthy containers."

# Sonsuz döngü — sürekli kontrol et
while true; do
  # "autoheal=true" label'ına sahip VE "unhealthy" durumundaki container'ları bul
  UNHEALTHY=$(docker ps --filter "label=autoheal=true" --filter "health=unhealthy" --format "{{.Names}}")
  if [ -n "$UNHEALTHY" ]; then
    # Sağlıksız container bulundu — her birini yeniden başlat
    for CONTAINER in $UNHEALTHY; do
      echo "$(date '+%Y-%m-%d %H:%M:%S') [AUTOHEAL] Restarting unhealthy container: $CONTAINER"
      docker restart "$CONTAINER"
    done
  fi
  # Bir sonraki kontrole kadar bekle
  sleep "$INTERVAL"
done
