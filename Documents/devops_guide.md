# 🐳 Transcendence DevOps Mimarisi — Kapsamlı Rehber

Bu doküman projenin tüm DevOps altyapısını, container'ları, monitoring sistemini ve log yönetimini açıklar.

---

## 📋 İçindekiler
1. [Genel Mimari](#genel-mimari)
2. [Container'lar ve Görevleri](#containerlar-ve-görevleri)
3. [Docker Compose Profilleri](#docker-compose-profilleri)
4. [Nginx Reverse Proxy](#nginx-reverse-proxy)
5. [Monitoring Sistemi (Prometheus + Grafana)](#monitoring-sistemi)
6. [ELK Stack (Log Yönetimi)](#elk-stack)
7. [Alert Sistemi](#alert-sistemi)
8. [Veri Akış Diyagramları](#veri-akış-diyagramları)
9. [Healthcheck Mekanizması](#healthcheck-mekanizması)
10. [Makefile Komutları](#makefile-komutları)
11. [Sık Sorulan Sorular](#sık-sorulan-sorular)

---

## Genel Mimari

Proje 3 katmandan oluşur:

```
┌─────────────────────────────────────────────────────┐
│                    KULLANICI                        │
│              (Tarayıcı: HTTPS:8443)                 │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              NGINX (Reverse Proxy)                  │
│    HTTP:80 → HTTPS:443 yönlendirme                  │
│    /         → Frontend (SvelteKit)                 │
│    /api/     → Backend (.NET)                       │
│    /grafana/ → Grafana Dashboard                    │
│    /kibana/  → Kibana (ELK)                         │
│    /prometheus/ → Prometheus UI                     │
└──────────────────────┬──────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              UYGULAMA KATMANI                       │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐  │
│  │ Frontend │  │ Backend  │  │   PostgreSQL DB   │  │
│  │ :3000    │  │ :5000    │  │   :5432           │  │
│  └──────────┘  └──────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              İZLEME & LOG KATMANI                   │
│  ┌─────────────────────────────────────────────┐    │
│  │ Monitoring: Prometheus + Grafana            │    │
│  │ + Node Exporter + cAdvisor                  │    │
│  │ + Postgres Exporter + Nginx Exporter        │    │
│  └─────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────┐    │
│  │ ELK: Filebeat → Logstash → Elasticsearch   │    │
│  │      → Kibana                               │    │
│  └─────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

---

## Container'lar ve Görevleri

### 🟢 Core Servisler (Her zaman çalışır)

| Container | Image | Port | Görevi |
|-----------|-------|------|--------|
| **transcendence-db** | `postgres:16-alpine` | 5432 | PostgreSQL veritabanı. Kullanıcı bilgileri, mesajlar, post'lar burada saklanır |
| **transcendence-backend** | Özel build | 5000 (internal) | .NET 10 Web API. REST endpoint'leri, SignalR WebSocket, JWT auth |
| **transcendence-frontend** | Özel build | 3000 (internal) | SvelteKit SSR uygulaması. Kullanıcı arayüzü |
| **transcendence-nginx** | Özel build | 8080→80, 8443→443 | Reverse proxy. Tüm trafik buradan geçer. SSL/TLS terminasyonu yapar |
| **transcendence-autoheal** | Özel build | — | Çöken container'ları otomatik yeniden başlatır. Docker socket'e bağlanır |

### 📈 Monitoring Servisleri (Profile: `monitoring`)

| Container | Image | Port | Görevi |
|-----------|-------|------|--------|
| **transcendence-prometheus** | `prom/prometheus` | 9090 | Metrik toplama motoru. Her 15 saniyede target'lardan metrik çeker |
| **transcendence-grafana** | `grafana/grafana` | 3001→3000 | Metrik görselleştirme. Dashboard'lar ve grafikler burada |
| **transcendence-node-exporter** | `prom/node-exporter` | 9100 (internal) | Sunucu metrikleri: CPU, RAM, disk, ağ (host düzeyinde) |
| **transcendence-cadvisor** | `gcr.io/cadvisor` | 8080 (internal) | Container metrikleri: Her container'ın CPU, RAM, ağ kullanımı |
| **transcendence-postgres-exporter** | `prometheuscommunity/postgres-exporter` | 9187 (internal) | PostgreSQL metrikleri: Aktif bağlantılar, sorgu süreleri, tablo boyutları |
| **transcendence-nginx-exporter** | `nginx/nginx-prometheus-exporter` | — | Nginx metrikleri: İstek sayısı, aktif bağlantılar, hata oranları |

### 📋 ELK Servisleri (Profile: `elk`)

| Container | Image | Port | Görevi |
|-----------|-------|------|--------|
| **transcendence-elasticsearch** | `elasticsearch:8.17.1` | 9200 | Log deposu. Logları indeksler ve aranabilir hale getirir |
| **transcendence-logstash** | `logstash:8.17.1` | 5044 | Log işleme motoru. Filebeat'ten gelen logları filtreler/zenginleştirir |
| **transcendence-kibana** | `kibana:8.17.1` | 5601 | Log görselleştirme arayüzü. Logları arayabilir, filtreleyebilirsin |
| **transcendence-filebeat** | `filebeat:8.17.1` | — | Log toplayıcı. Tüm Docker container loglarını okur ve Logstash'e gönderir |

---

## Docker Compose Profilleri

Docker Compose'da **profile** sistemi kullanılıyor. Bu sayede monitoring ve ELK servisleri isteğe bağlı başlatılır:

```yaml
# Core servisler → profile yok → her zaman başlar
database, backend, frontend, nginx, autoheal

# Monitoring → "monitoring" profile
prometheus, grafana, node-exporter, cadvisor, postgres-exporter, nginx-exporter

# ELK → "elk" profile
elasticsearch, logstash, kibana, filebeat
```

**Neden profil kullanılır?** Monitoring ve ELK çok fazla RAM tüketir. Geliştirme sırasında sadece core servisleri çalıştırmak yeterlidir. İzleme gerektiğinde profil aktifleştirilir.

---

## Nginx Reverse Proxy

Nginx tüm trafiğin giriş noktasıdır. Dışarıdan sadece Nginx'in portları (8080/8443) açıktır.

### Yönlendirme Tablosu

| URL Yolu | Hedef | Açıklama |
|-----------|-------|----------|
| `/` | `frontend:3000` | Ana sayfa (SvelteKit) |
| `/api/` | `backend:5000` | REST API endpoint'leri |
| `/uploads/` | `backend:5000` | Kullanıcı yükleme dosyaları |
| `/swagger` | `backend:5000` | API dokümantasyonu |
| `/chathub` | `backend:5000` | WebSocket (SignalR chat) |
| `/grafana/` | `grafana:3000` | Monitoring dashboard |
| `/kibana/` | `kibana:5601` | Log arayüzü |
| `/prometheus/` | `prometheus:9090` | Metrik arayüzü |

### SSL/TLS
- Nginx self-signed sertifika üretir (build sırasında `openssl` ile)
- HTTP (port 80) → HTTPS (port 443) otomatik yönlendirme yapar
- Dış dünyadan: `https://localhost:8443`

### Dynamic Resolver
Grafana, Kibana, Prometheus gibi profil servisleri her zaman çalışmayabilir. Nginx bunlara `resolver 127.0.0.11` (Docker DNS) ile bağlanır. Servis yoksa 502 döner ama Nginx çökmez.

---

## Monitoring Sistemi

### Veri Akışı

```
┌────────────────┐     scrape (15s)     ┌─────────────┐     query      ┌─────────┐
│  Node Exporter │ ──────────────────→  │             │  ←───────────  │         │
│  cAdvisor      │ ──────────────────→  │  Prometheus │  ────────────→ │ Grafana │
│  Postgres Exp. │ ──────────────────→  │   :9090     │                │  :3000  │
│  Nginx Exp.    │ ──────────────────→  │             │                │         │
└────────────────┘                      └─────────────┘                └─────────┘
     Exporter'lar                        Metrik Deposu                 Görselleştirme
```

### Prometheus Nasıl Çalışır?
1. **Scrape (Çekme)**: Prometheus her 15 saniyede bir target'ların `/metrics` endpoint'ini çeker
2. **Depolama**: Metrikleri zaman serisi olarak kendi veritabanında saklar (7 gün, max 500MB)
3. **Alert Değerlendirme**: Her 15 saniyede alert kurallarını kontrol eder
4. **PromQL**: Metrikleri sorgulamak için özel bir dil kullanılır

### Prometheus Target'ları ve Ne Toplar?

| Target | Topladığı Metrikler |
|--------|-------------------|
| **prometheus** | Kendi iç metrikleri (bellek, goroutine sayısı) |
| **node-exporter** | Host CPU, RAM, disk I/O, ağ trafiği, dosya sistemi |
| **cadvisor** | Container başına CPU, RAM, ağ, disk kullanımı |
| **postgres-exporter** | DB bağlantı sayısı, sorgu süreleri, tablo/veritabanı boyutları |
| **nginx-exporter** | HTTP istek sayısı, aktif bağlantılar, istek/saniye |

### Grafana Dashboard Panelleri

| Panel | Metrik | Açıklama |
|-------|--------|----------|
| Container CPU Usage | `container_cpu_usage_seconds_total` | Her container'ın CPU kullanım yüzdesi |
| Container Memory Usage | `container_memory_usage_bytes` | Her container'ın RAM kullanımı (byte) |
| Nginx HTTP Request Rate | `nginx_http_requests_total` | Saniyedeki HTTP istek sayısı |
| Nginx Active Connections | `nginx_connections_active` | Anlık aktif bağlantı sayısı |
| Prometheus Targets Status | `up` | Her target'ın UP/DOWN durumu |
| PostgreSQL Active Connections | `pg_stat_activity_count` | Veritabanı aktif bağlantı sayısı |
| PostgreSQL Database Size | `pg_database_size_bytes` | Veritabanı boyutu |
| System Load Average | `node_load1` | Sunucu yük ortalaması (1 dakika) |
| Container Network Receive | `container_network_receive_bytes_total` | Container'ların aldığı ağ trafiği |
| Container Network Transmit | `container_network_transmit_bytes_total` | Container'ların gönderdiği ağ trafiği |
| Alerts Status | — | Aktif alert'lerin listesi |

### Grafana Erişim
- **URL**: `https://localhost:8443/grafana/`
- **Kullanıcı**: `admin`
- **Şifre**: `transcendence42` (`.env` ile değiştirilebilir)

---

## ELK Stack

### ELK Nedir?
- **E**lasticsearch: Log depolama ve arama motoru
- **L**ogstash: Log işleme ve filtreleme
- **K**ibana: Log görselleştirme arayüzü
- **+ Filebeat**: Hafif log toplayıcı agent

### Veri Akışı

```
┌──────────────┐          ┌──────────┐          ┌───────────────┐          ┌─────────┐
│ Docker       │  log     │          │ process  │               │  query   │         │
│ Container    │ ───────→ │ Filebeat │ ───────→ │   Logstash    │ ───────→ │Elastic- │
│ Log Files    │          │          │  :5044   │  (filter +    │          │ search  │
│              │          │          │          │   enrich)     │          │  :9200  │
└──────────────┘          └──────────┘          └───────────────┘          └────┬────┘
  /var/lib/docker/                                                              │
  containers/*/*.log                                                            │
                                                                          ┌─────▼────┐
                                                                          │  Kibana  │
                                                                          │  :5601   │
                                                                          │ (arama & │
                                                                          │  görsel) │
                                                                          └──────────┘
```

### Her Bileşen Ne Yapar?

#### Filebeat
- Docker container'larının log dosyalarını (`/var/lib/docker/containers/*/*.log`) okur
- Her log satırına **Docker metadata** ekler (container adı, image, label'lar)
- Logları Logstash'e (port 5044) gönderir
- Çok hafiftir (~10MB RAM), container'larda çalışmak için tasarlanmıştır

#### Logstash
- Filebeat'ten gelen ham logları alır
- **Filter** aşamasında logları zenginleştirir:
  - `container.name` → `service_name` alanına kopyalar
  - Böylece Kibana'da `service_name: transcendence-backend` diye filtreleme yapılabilir
- İşlenen logları Elasticsearch'e yazar
- Her gün yeni bir index oluşturur: `logstash-2026.07.16`

#### Elasticsearch
- Logları indeksler ve tam metin araması yapılabilir hale getirir
- Tek node olarak çalışır (`discovery.type=single-node`)
- Güvenlik kapalı (`xpack.security.enabled=false`) — dahili kullanım

#### Kibana
- Web arayüzünden logları arama, filtreleme ve görselleştirme
- **Discover** sekmesi: Canlı log akışı
- **Dashboard**: Özel log grafikleri oluşturulabilir

### Kibana İlk Kurulum
1. `https://localhost:8443/kibana/` adresine git
2. **Management → Stack Management → Data Views**
3. Index pattern: `logstash-*`
4. Time field: `@timestamp`
5. **Discover** sekmesinden logları görüntüle

---

## Alert Sistemi

### Prometheus Alert'leri

Alert'ler [alert_rules.yml](file:///home/omadali/dosyaubuntu/transendence2/Devops/monitoring/prometheus/alert_rules.yml) dosyasında tanımlıdır.

#### Alert Durumları

| Durum | Renk | Anlamı |
|-------|------|--------|
| **inactive** | 🟢 Yeşil | Alert koşulu sağlanmıyor → **HER ŞEY NORMAL** |
| **pending** | 🟡 Sarı | Koşul sağlandı ama "for" süresi henüz dolmadı |
| **firing** | 🔴 Kırmızı | Alert tetiklendi! Müdahale gerekli |

> **"Alert inactive" demek sorun yok demektir!** Prometheus sürekli kuralları kontrol eder, koşul sağlanmadığında "inactive" gösterir. Bu beklenen ve istenen durumdur.

#### Tanımlı Alert Kuralları

| Alert | Koşul | Süre | Severity |
|-------|-------|------|----------|
| **InstanceDown** | Herhangi bir servis çöktü (`up == 0`) | 1 dk | 🔴 critical |
| **HighCpuUsage** | Container CPU > %80 | 5 dk | 🟡 warning |
| **HighMemoryUsage** | Container RAM > %85 limit | 5 dk | 🟡 warning |
| **DatabaseDown** | PostgreSQL erişilemez | 1 dk | 🔴 critical |
| **DatabaseHighConnections** | DB bağlantı > 80 | 5 dk | 🟡 warning |
| **NginxHighErrorRate** | Nginx 5xx hata > %5 | 5 dk | 🟡 warning |
| **DiskSpaceRunningLow** | Disk boş alan < %15 | 5 dk | 🟡 warning |

---

## Veri Akış Diyagramları

### Kullanıcı İsteği Akışı
```
Kullanıcı → HTTPS:8443 → Nginx → Frontend (sayfa) / Backend (API)
                                    ↓
                                PostgreSQL (veri)
```

### Monitoring Veri Akışı
```
Container'lar → cAdvisor → Prometheus ← Node Exporter (host)
                              ↓            ← Postgres Exporter (DB)
                           Grafana          ← Nginx Exporter (HTTP)
```

### Log Akışı
```
Container stdout/stderr → Docker log dosyaları → Filebeat → Logstash → Elasticsearch → Kibana
```

---

## Healthcheck Mekanizması

Her container'ın sağlık kontrolü tanımlıdır. Docker bu kontrolleri düzenli olarak çalıştırır:

| Container | Kontrol Yöntemi | Aralık |
|-----------|----------------|--------|
| Database | `pg_isready -U postgres` | 30s |
| Backend | `wget http://127.0.0.1:5000/swagger/index.html` | 30s |
| Frontend | `wget http://127.0.0.1:3000/` | 30s |
| Nginx | `wget http://127.0.0.1:8888/nginx_status` | 30s |
| Prometheus | `wget http://127.0.0.1:9090/prometheus/-/healthy` | 30s |
| Grafana | `wget http://127.0.0.1:3000/api/health` | 30s |
| Elasticsearch | `curl http://127.0.0.1:9200` | 30s |
| Node Exporter | `wget http://127.0.0.1:9100/metrics` | 30s |
| cAdvisor | `wget http://127.0.0.1:8080/healthz` | 30s |
| Postgres Exporter | `wget http://127.0.0.1:9187/metrics` | 30s |

### Autoheal Container
- `autoheal=true` label'ı olan container'ları izler
- Bir container "unhealthy" durumuna düşerse **otomatik olarak restart eder**
- Docker socket'e (`/var/run/docker.sock`) bağlanarak container'ları yönetir
- 30 saniyede bir kontrol yapar, ilk 60 saniye bekleme süresi verir

---

## Makefile Komutları

| Komut | Ne Yapar |
|-------|----------|
| `make nuke` | Core servisleri durdur, volume'ları sil, upload'ları temizle, sıfırdan rebuild et ve başlat. **Image silmez.** |
| `make full-nuke` | Yukarıdakinin aynısı + monitoring + ELK dahil. **Image silmez.** |
| `make nuke-extra` | Full-nuke + **tüm proje image'larını siler**. En temiz başlangıç. |
| `make elk-up` | ELK stack'i başlat |
| `make elk-down` | ELK stack'i durdur |
| `make elk-logs` | ELK loglarını izle |
| `make monitoring-up` | Monitoring servislerini başlat |
| `make monitoring-down` | Monitoring servislerini durdur |
| `make monitoring-logs` | Monitoring loglarını izle |
| `make full-up` | Core + Monitoring + ELK hepsini başlat |
| `make full-down` | Hepsini durdur |

---

## Sık Sorulan Sorular

### ❓ "Alert inactive" ne demek?
**Her şey normal demek.** Alert koşulu (örn. CPU > %80) sağlanmadığı için tetiklenmemiş. Bu iyi bir şey.

### ❓ Grafana'da "No Data" görüyorum
Olası nedenler:
1. **Datasource bağlantısı**: Prometheus URL'i yanlış olabilir (sub-path dahil olmalı)
2. **Dashboard UID**: Panel'lerin datasource UID'si boşsa veri çekemez
3. **Prometheus henüz yeterli veri toplamadı**: İlk başlatmada 1-2 dakika bekle
4. **Servis çalışmıyor**: Target UP durumunda mı kontrol et

### ❓ Kibana'da Data View oluşturamıyorum
1. Logstash çalışıyor mu kontrol et: `docker logs transcendence-logstash`
2. OutOfMemoryError varsa heap artır (docker-compose.yml'de `LS_JAVA_OPTS`)
3. Elasticsearch'te index var mı: `curl http://localhost:9200/_cat/indices`
4. `logstash-*` index'i yoksa Logstash veri işleyememiştir

### ❓ Bir container sürekli restart ediyor
1. `docker logs <container-name>` ile hatayı kontrol et
2. Autoheal unhealthy container'ları restart eder — bu beklenen davranış
3. Healthcheck'in neden fail olduğunu anla ve root cause'u düzelt

### ❓ Docker compose profil sistemi nasıl çalışır?
- Profile olmayan servisler **her zaman** başlar
- `--profile monitoring` eklenirse monitoring servisleri de başlar
- `--profile elk` eklenirse ELK servisleri de başlar
- İkisi birden: `--profile monitoring --profile elk`

---

## Erişim URL'leri Özeti

| Servis | URL |
|--------|-----|
| Uygulama (Frontend) | `https://localhost:8443/` |
| API (Swagger) | `https://localhost:8443/swagger` |
| Grafana | `https://localhost:8443/grafana/` |
| Prometheus | `https://localhost:8443/prometheus/` |
| Kibana | `https://localhost:8443/kibana/` |
