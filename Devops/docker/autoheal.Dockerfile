# ============================================================
# Autoheal Dockerfile — Sağlık Kontrolü & Otomatik Yeniden Başlatma
# ============================================================
# Bu Dockerfile, autoheal.sh script'ini çalıştıran minimal bir
# container oluşturur. Docker CLI kullanarak "unhealthy" durumundaki
# container'ları otomatik olarak yeniden başlatır.
# Docker socket'e erişim gerektirir (/var/run/docker.sock)
# ============================================================

# docker:cli — sadece Docker komut satırı aracını içerir (~15MB)
# Docker daemon içermez, sadece host'un Docker'ına komut gönderir
FROM docker:cli

# Autoheal script'ini container'a kopyala
# autoheal.sh: Belirli aralıklarla container sağlık durumlarını kontrol eder
# "autoheal=true" label'ına sahip ve "unhealthy" olan container'ları restart eder
COPY autoheal.sh /autoheal.sh
RUN chmod +x /autoheal.sh

# Container başlatıldığında autoheal script'ini çalıştır
ENTRYPOINT ["/autoheal.sh"]
