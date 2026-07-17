# Monitoring & ELK Stack — Detaylı Bileşen Açıklaması

Bu doküman, `docker-compose.yml` ve `nginx.conf` dosyalarındaki **monitoring** ve **ELK** bileşenlerinin her birinin ne yaptığını, hangi porta bağlandığını, hangi bilgileri ürettiğini ve bu bilgilerin nasıl işlendiğini detaylı olarak açıklar.

---

## 📐 Genel Mimari Şema

```mermaid
graph TB
    subgraph "Monitoring Stack (profile: monitoring)"
        NE["Node Exporter<br/>:9100"] -->|"/metrics"| PROM["Prometheus<br/>:9090"]
        CA["cAdvisor<br/>:8080"] -->|"/metrics"| PROM
        PE["Postgres Exporter<br/>:9187"] -->|"/metrics"| PROM
        NX_EXP["Nginx Exporter<br/>:9113"] -->|"/metrics"| PROM
        PROM -->|"veri kaynağı"| GRAF["Grafana<br/>:3000"]
    end

    subgraph "ELK Stack (profile: elk)"
        FB["Filebeat"] -->|"port 5044"| LS["Logstash<br/>:5044"]
        LS -->|"port 9200"| ES["Elasticsearch<br/>:9200"]
        ES -->|"veri kaynağı"| KIB["Kibana<br/>:5601"]
    end

    subgraph "Uygulama Servisleri"
        DB["PostgreSQL<br/>:5432"]
        BACK["Backend<br/>:5000"]
        FRONT["Frontend<br/>:3000"]
        NGX["Nginx<br/>:80/:443/:8888"]
    end

    PE -.->|"SQL sorguları"| DB
    NX_EXP -.->|"stub_status"| NGX
    CA -.->|"docker.sock"| BACK
    CA -.->|"docker.sock"| FRONT
    CA -.->|"docker.sock"| DB
    CA -.->|"docker.sock"| NGX
    FB -.->|"container logları"| BACK
    FB -.->|"container logları"| FRONT
    FB -.->|"container logları"| DB
    FB -.->|"container logları"| NGX

    NGX -->|"/grafana/"| GRAF
    NGX -->|"/kibana"| KIB
    NGX -->|"/prometheus"| PROM
```

---

## 🔶 BÖLÜM 1: NGINX (Reverse Proxy & Giriş Noktası)

### Dosyalar
- [docker-compose.yml — nginx servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L89-L113)
- [nginx.conf](file:///home/omadali/dosyaubuntu/transendence/Devops/nginx/nginx.conf)

### Ne Yapar?
Nginx, projenin **tek giriş noktasıdır (reverse proxy)**. Dış dünyadan gelen tüm HTTP/HTTPS isteklerini alır ve iç servislere yönlendirir. Aynı zamanda monitoring ve ELK araçlarına erişim sağlar.

### Port Yapısı

| Port | Protokol | Açıklama |
|------|----------|----------|
| `80` (host → container) | HTTP | Gelen tüm HTTP isteklerini **HTTPS'e yönlendirir** (301 redirect) |
| `443` (host → container) | HTTPS/SSL | Asıl trafik portu. Frontend, Backend, Grafana, Kibana, Prometheus hepsine buradan ulaşılır |
| `8888` (sadece iç ağ) | HTTP | `stub_status` endpoint'i — nginx-exporter bu portu okur |

### Yönlendirme Tablosu (nginx.conf)

| URL Yolu | Hedef Servis | Hedef Port | Açıklama |
|-----------|-------------|------------|----------|
| `/` | `frontend:3000` | 3000 | React/Svelte frontend uygulaması |
| `/api/` | `backend:5000` | 5000 | .NET Backend API |
| `/uploads/` | `backend:5000` | 5000 | Statik dosya servisi |
| `/swagger` | `backend:5000` | 5000 | API dokümantasyonu |
| `/chathub` | `backend:5000` | 5000 | WebSocket (SignalR) bağlantısı |
| `/grafana/` | `grafana:3000` | 3000 | Monitoring dashboard'ları |
| `/kibana` | `kibana:5601` | 5601 | Log analiz arayüzü |
| `/prometheus` | `prometheus:9090` | 9090 | Metrik sorgulama arayüzü |
| `/nginx_status` (port 8888) | Kendi içi | — | Nginx istatistikleri (sadece exporter için) |

### `stub_status` Endpoint'i (Port 8888)
```
server {
    listen 8888;
    location /nginx_status {
        stub_status on;    ← Aktif bağlantı, toplam istek gibi istatistikleri döndürür
        access_log off;    ← Bu istekler access log'a yazılmaz (gereksiz log kirliliği önlenir)
    }
}
```
Bu endpoint **nginx-exporter** tarafından okunur ve Prometheus formatına çevrilir.

### `resolver 127.0.0.11` Ne Demek?
Grafana, Kibana ve Prometheus location bloklarında `resolver 127.0.0.11` kullanılır. Bu, Docker'ın dahili DNS sunucusudur. Neden gerekli? Çünkü bu servisler **profile** ile çalışır — her zaman ayakta olmayabilirler. Normal `proxy_pass` kullanılsaydı, servis yokken Nginx tamamen çökerdi. Bu yöntemle servis yoksa sadece 502 hata döner, Nginx çalışmaya devam eder.

---

## 🔶 BÖLÜM 2: MONITORING STACK

Bu bölümdeki tüm servisler `profiles: ["monitoring"]` ile tanımlıdır. Yani sadece `docker compose --profile monitoring up` komutuyla ayağa kalkarlar.

---

### 2.1 — Prometheus (Metrik Deposu & Sorgulama Motoru)

#### Dosyalar
- [docker-compose.yml — prometheus servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L115-L144)
- [prometheus.yml](file:///home/omadali/dosyaubuntu/transendence/Devops/monitoring/prometheus/prometheus.yml)
- [alert_rules.yml](file:///home/omadali/dosyaubuntu/transendence/Devops/monitoring/prometheus/alert_rules.yml)

#### Ne Yapar?
Prometheus, **metrik toplama ve saklama motorudur**. Her **15 saniyede** bir, tanımlı hedeflerin (targets) `/metrics` endpoint'ine HTTP GET isteği atar ve dönen metrikleri zaman serisi veritabanına yazar. Buna "**scraping**" denir.

#### Port & Erişim
- **Port 9090** (hem host'a açık hem iç ağ)
- Web arayüzü: `https://localhost:HTTPS_PORT/prometheus` (Nginx üzerinden)
- Alt yol: `--web.route-prefix=/prometheus` ile `/prometheus` altında çalışır

#### Kimlerden Veri Toplar? (Scrape Targets)

| Job Name | Hedef Servis | Port | Topladığı Metrikler |
|----------|-------------|------|---------------------|
| `prometheus` | Kendisi (`localhost:9090`) | 9090 | Prometheus'un kendi iç metrikleri (sorgu sayısı, bellek kullanımı) |
| `node-exporter` | `node-exporter:9100` | 9100 | Host makinenin CPU, RAM, disk, ağ metrikleri |
| `cadvisor` | `cadvisor:8080` | 8080 | Her Docker container'ın CPU, RAM, ağ, disk I/O metrikleri |
| `postgres-exporter` | `postgres-exporter:9187` | 9187 | PostgreSQL bağlantı sayısı, sorgu süreleri, tablo boyutları |
| `nginx-exporter` | `nginx-exporter:9113` | 9113 | Nginx toplam istek, aktif bağlantı, istek/saniye |

#### Veri Saklama
- Veriler `/prometheus` dizinine yazılır (Docker volume: `prometheus_data`)
- **7 gün** veya **500MB** dolunca eski veriler silinir
- `--web.enable-lifecycle`: API ile config yeniden yükleme desteği

#### Alert Kuralları
Prometheus her 15 saniyede alert kurallarını değerlendirir:

| Alert | Tetiklenme Koşulu | Süre | Severity |
|-------|-------------------|------|----------|
| **InstanceDown** | Herhangi bir exporter'a ulaşılamıyor (`up == 0`) | 1 dk | 🔴 critical |
| **HighCpuUsage** | Container CPU > %80 | 5 dk | 🟡 warning |
| **HighMemoryUsage** | Container RAM > %85 (limit varsa) | 5 dk | 🟡 warning |
| **DatabaseDown** | PostgreSQL erişilemez (`pg_up == 0`) | 1 dk | 🔴 critical |
| **DatabaseHighConnections** | DB bağlantısı > 80 (max 100) | 5 dk | 🟡 warning |
| **NginxHighErrorRate** | 5xx hata oranı > %5 | 5 dk | 🟡 warning |
| **DiskSpaceRunningLow** | Disk boş alanı < %15 | 5 dk | 🟡 warning |

---

### 2.2 — Grafana (Görselleştirme Dashboard'u)

#### Dosyalar
- [docker-compose.yml — grafana servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L146-L175)
- [datasource.yml](file:///home/omadali/dosyaubuntu/transendence/Devops/monitoring/grafana/provisioning/datasources/datasource.yml)

#### Ne Yapar?
Grafana, Prometheus'taki metrikleri **grafikler, tablolar ve panellerle görselleştirir**. Kendi başına veri toplamaz, sadece Prometheus'a PromQL sorguları atarak veriyi çeker ve gösterir.

#### Port & Erişim
- **İç port 3000** (host'a açılmamış, sadece Nginx üzerinden erişilir)
- Web arayüzü: `https://localhost:HTTPS_PORT/grafana/`
- `GF_SERVER_SERVE_FROM_SUB_PATH: true` → `/grafana/` alt yolunda çalışır

#### Veri Kaynağı Bağlantısı
```yaml
url: http://prometheus:9090/prometheus    ← Docker iç ağ üzerinden Prometheus'a bağlanır
access: proxy                             ← Grafana sunucusu Prometheus'a bağlanır (tarayıcı değil)
```

#### Provisioning (Otomatik Yapılandırma)
Grafana başlatıldığında şunlar **otomatik olarak** yüklenir (manuel ayar gerekmez):
- **Datasource**: Prometheus veri kaynağı (`prometheus-ds` UID ile)
- **Dashboard'lar**: `/var/lib/grafana/dashboards` dizinindeki JSON dosyaları

#### Veri Akışı
```
Exporter'lar → Prometheus → (PromQL sorgusu) → Grafana → Tarayıcıdaki Grafikler
```

---

### 2.3 — Node Exporter (Host/Makine Metrikleri)

#### Dosya
- [docker-compose.yml — node-exporter servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L177-L201)

#### Ne Yapar?
Node Exporter, Docker'ın çalıştığı **host makinenin (sunucu/VM) fiziksel kaynak metriklerini** toplar. Container-level değil, **makine-level** metrikler üretir.

#### Port & Bağlantı
- **Port 9100** (iç ağ, host'a açılmamış)
- Prometheus `node-exporter:9100/metrics` adresinden 15 saniyede bir çeker

#### Nasıl Çalışır? (Volume Mount'lar)
```yaml
volumes:
  - /proc:/host/proc:ro     ← Linux process bilgileri (CPU, RAM kullanımı)
  - /sys:/host/sys:ro       ← Kernel/donanım bilgileri (disk, ağ arayüzleri)
  - /:/rootfs:ro            ← Dosya sistemi bilgileri (disk doluluk oranları)
```
Bu dizinleri **read-only** mount ederek host'un iç bilgilerine erişir. Container'ın içinden host'u "görmesini" sağlar.

#### Ürettiği Metrik Örnekleri
| Metrik | Açıklama |
|--------|----------|
| `node_cpu_seconds_total` | Her CPU çekirdeğinin kullanım süresi |
| `node_memory_MemTotal_bytes` | Toplam fiziksel RAM |
| `node_memory_MemAvailable_bytes` | Kullanılabilir RAM |
| `node_filesystem_avail_bytes` | Diskteki boş alan |
| `node_filesystem_size_bytes` | Disk toplam boyutu |
| `node_network_receive_bytes_total` | Ağdan alınan toplam bayt |
| `node_network_transmit_bytes_total` | Ağa gönderilen toplam bayt |
| `node_disk_read_bytes_total` | Diskten okunan toplam bayt |

---

### 2.4 — cAdvisor (Container Metrikleri)

#### Dosya
- [docker-compose.yml — cadvisor servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L203-L233)

#### Ne Yapar?
cAdvisor (Container Advisor), **her bir Docker container'ın kaynak tüketimini ayrı ayrı izler**. Node Exporter makine genelini izlerken, cAdvisor "hangi container ne kadar CPU/RAM kullanıyor?" sorusuna cevap verir.

#### Port & Bağlantı
- **Port 8080** (iç ağ, host'a açılmamış)
- Prometheus `cadvisor:8080/metrics` adresinden 15 saniyede bir çeker

#### Nasıl Çalışır? (Volume Mount'lar)
```yaml
volumes:
  - /:/rootfs:ro                        ← Host dosya sistemi (disk kullanımı)
  - /var/run:/var/run:rw                ← Docker socket (container listesi) ⚠️ rw gerekli
  - /sys:/sys:ro                        ← cgroup bilgileri (kaynak limitleri)
  - /var/lib/docker/:/var/lib/docker:ro ← Container image/layer bilgileri
  - /dev/disk/:/dev/disk:ro             ← Disk aygıt bilgileri
  - /sys/fs/cgroup:/sys/fs/cgroup:ro    ← Control groups (CPU/RAM limitleri)
devices:
  - /dev/kmsg:/dev/kmsg                 ← Kernel mesaj tamponu
```

`privileged: true` → cAdvisor'ın tüm container bilgilerine erişebilmesi için gerekli.

#### Komut Parametreleri
```yaml
command:
  - "--docker_only=true"                ← Sadece Docker container'ları izle (system cgroup'ları değil)
  - "--store_container_labels=true"     ← Container label'larını da metrik olarak sakla
  - "--housekeeping_interval=15s"       ← 15 saniyede bir veri topla
```

#### Hangi Container'ları İzler?
**Tüm Docker container'larını otomatik olarak izler**:
- `transcendence-db` (PostgreSQL)
- `transcendence-backend`
- `transcendence-frontend`
- `transcendence-nginx`
- `transcendence-prometheus`
- `transcendence-grafana`
- `transcendence-node-exporter`
- Ve compose'daki diğer tüm container'lar

#### Ürettiği Metrik Örnekleri
| Metrik | Açıklama |
|--------|----------|
| `container_cpu_usage_seconds_total` | Container'ın toplam CPU kullanım süresi |
| `container_memory_usage_bytes` | Container'ın o anki RAM kullanımı |
| `container_memory_working_set_bytes` | Container'ın gerçek çalışma belleği |
| `container_spec_memory_limit_bytes` | Container'a atanan RAM limiti |
| `container_network_receive_bytes_total` | Container'ın ağdan aldığı toplam bayt |
| `container_network_transmit_bytes_total` | Container'ın ağa gönderdiği toplam bayt |
| `container_fs_usage_bytes` | Container'ın disk kullanımı |
| `container_fs_reads_bytes_total` | Diskten okunan bayt |

---

### 2.5 — Postgres Exporter (Veritabanı Metrikleri)

#### Dosya
- [docker-compose.yml — postgres-exporter servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L235-L255)

#### Ne Yapar?
PostgreSQL veritabanına **SQL sorguları atarak** veritabanının iç durumunu metrik olarak dışarı verir. Bağlantı sayısı, sorgu performansı, tablo boyutları gibi DB-spesifik bilgileri toplar.

#### Port & Bağlantı
- **Port 9187** (iç ağ) → Prometheus bu portu scrape eder
- **PostgreSQL'e bağlantı**: `database:5432` (Docker iç ağ)

#### Bağlantı Detayı
```yaml
DATA_SOURCE_NAME: "postgresql://postgres:postgres@database:5432/transcendence?sslmode=disable"
```
- `database` → docker-compose'daki PostgreSQL servis adı
- Port `5432` → PostgreSQL'in standart portu
- `sslmode=disable` → İç ağda SSL gerekmez

#### Ürettiği Metrik Örnekleri
| Metrik | Açıklama |
|--------|----------|
| `pg_up` | Veritabanı erişilebilir mi? (1=evet, 0=hayır) |
| `pg_stat_activity_count` | Aktif veritabanı bağlantı sayısı |
| `pg_stat_database_tup_fetched` | Okunan satır sayısı |
| `pg_stat_database_tup_inserted` | Eklenen satır sayısı |
| `pg_stat_database_tup_updated` | Güncellenen satır sayısı |
| `pg_stat_database_tup_deleted` | Silinen satır sayısı |
| `pg_database_size_bytes` | Veritabanı boyutu (bayt) |
| `pg_stat_database_conflicts` | Çakışma sayısı |
| `pg_stat_database_deadlocks` | Deadlock sayısı |

---

### 2.6 — Nginx Exporter (Web Sunucu Metrikleri)

#### Dosya
- [docker-compose.yml — nginx-exporter servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L257-L270)

#### Ne Yapar?
Nginx'in `stub_status` sayfasını okuyarak Nginx'in performans metriklerini Prometheus formatına çevirir.

#### Port & Bağlantı Zinciri
```
Nginx (port 8888, /nginx_status) → nginx-exporter (port 9113, /metrics) → Prometheus
```

1. Nginx, **port 8888**'de `stub_status` modülü ile ham istatistik sunar
2. nginx-exporter, `http://nginx:8888/nginx_status` adresini okur
3. Bu ham veriyi Prometheus formatına çevirir ve **port 9113**'te `/metrics` olarak sunar
4. Prometheus, `nginx-exporter:9113` adresinden bu metrikleri çeker

#### Komut
```yaml
command:
  - "--nginx.scrape-uri=http://nginx:8888/nginx_status"
```

#### Ürettiği Metrik Örnekleri
| Metrik | Açıklama |
|--------|----------|
| `nginx_connections_active` | O anki aktif bağlantı sayısı |
| `nginx_connections_accepted` | Kabul edilen toplam bağlantı |
| `nginx_connections_handled` | İşlenen toplam bağlantı |
| `nginx_connections_reading` | İstek okunan bağlantılar |
| `nginx_connections_writing` | Yanıt yazılan bağlantılar |
| `nginx_connections_waiting` | Bekleyen (idle) bağlantılar |
| `nginx_http_requests_total` | Toplam HTTP istek sayısı |

---

## 🔶 BÖLÜM 3: MONITORING VERİ AKIŞI ÖZETİ

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        MONITORING VERİ AKIŞI                           │
│                                                                        │
│  Host Makine ──────► Node Exporter (:9100) ──────┐                     │
│                                                   │                    │
│  Docker Container'lar ► cAdvisor (:8080) ─────────┤                    │
│                                                   ├──► Prometheus      │
│  PostgreSQL (:5432) ► Postgres Exporter (:9187) ──┤      (:9090)       │
│                                                   │        │           │
│  Nginx (:8888) ────► Nginx Exporter (:9113) ──────┘        │           │
│                                                             │           │
│                                                     PromQL Sorguları   │
│                                                             │           │
│                                                             ▼           │
│                                                      Grafana (:3000)   │
│                                                        Dashboard'lar   │
│                                                             │           │
│                                                             ▼           │
│                                                     Nginx (/grafana/)  │
│                                                             │           │
│                                                             ▼           │
│                                                      Kullanıcı Tarayıcı│
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔵 BÖLÜM 4: ELK STACK

Bu bölümdeki tüm servisler `profiles: ["elk"]` ile tanımlıdır. Yani sadece `docker compose --profile elk up` komutuyla ayağa kalkarlar.

> [!IMPORTANT]
> Monitoring stack **sayısal metrikleri** toplar (CPU %'si, istek sayısı vs.), ELK stack ise **log metinlerini** toplar (hata mesajları, istek detayları, debug çıktıları).

---

### 4.1 — Filebeat (Log Toplayıcı — Veri Giriş Noktası)

#### Dosyalar
- [docker-compose.yml — filebeat servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L347-L362)
- [filebeat.yml](file:///home/omadali/dosyaubuntu/transendence/Devops/monitoring/elk/filebeat/filebeat.yml)

#### Ne Yapar?
Filebeat, **tüm Docker container'larının log dosyalarını otomatik olarak toplar** ve Logstash'e gönderir. Çok hafif bir agent'tır — kendisi log işleme yapmaz, sadece toplar ve iletir.

#### Nasıl Çalışır?

1. **Log dosyalarını okur**:
```yaml
paths:
  - '/var/lib/docker/containers/*/*.log'
```
Docker, her container için `/var/lib/docker/containers/<container-id>/<container-id>-json.log` dosyasına log yazar. Filebeat bu dosyaları izler.

2. **Docker metadata ekler**:
```yaml
processors:
  - add_docker_metadata:
      host: "unix:///var/run/docker.sock"
```
Docker socket'e bağlanarak her log satırına **container adı, image adı, label'lar** gibi ek bilgileri ekler. Böylece "bu log hangi container'dan geldi?" sorusu cevaplanır.

3. **Logstash'e gönderir**:
```yaml
output.logstash:
  hosts: ["logstash:5044"]
```

#### Volume Mount'lar
```yaml
volumes:
  - ./Devops/monitoring/elk/filebeat/filebeat.yml:/usr/share/filebeat/filebeat.yml:ro  ← Config dosyası
  - /var/lib/docker/containers:/var/lib/docker/containers:ro  ← Container log dosyaları
  - /var/run/docker.sock:/var/run/docker.sock:ro              ← Docker API (metadata için)
```

#### Hangi Container'ların Loglarını Toplar?
**Tüm container'ların loglarını toplar** (seçici değildir):
- `transcendence-db` → SQL sorguları, bağlantı hataları
- `transcendence-backend` → API istekleri, .NET logları, hata stack trace'leri
- `transcendence-frontend` → Build logları, SSR hataları
- `transcendence-nginx` → Access log, error log
- `transcendence-prometheus`, `transcendence-grafana` vb. monitoring servisleri de dahil

---

### 4.2 — Logstash (Log İşleme Motoru — Pipeline)

#### Dosyalar
- [docker-compose.yml — logstash servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L310-L326)
- [logstash.conf](file:///home/omadali/dosyaubuntu/transendence/Devops/monitoring/elk/logstash/logstash.conf)

#### Ne Yapar?
Logstash, Filebeat'ten gelen **ham logları alır**, **işler/zenginleştirir** (filter) ve **Elasticsearch'e yazar**. Bir "pipeline" olarak 3 aşamada çalışır:

#### Pipeline Aşamaları

**1. INPUT (Giriş) — Port 5044**
```ruby
input {
  beats {
    port => 5044    ← Filebeat bu porta bağlanarak logları gönderir
  }
}
```
Filebeat, Beats protokolü üzerinden port 5044'e bağlanır ve topladığı logları Logstash'e iletir.

**2. FILTER (İşleme/Zenginleştirme)**
```ruby
filter {
  if [container][name] {
    mutate {
      add_field => { "service_name" => "%{[container][name]}" }
    }
  }
}
```
Gelen log'da container adı varsa, `service_name` adında yeni bir alan oluşturur. Böylece Kibana'da `service_name: transcendence-backend` şeklinde filtreleme yapılabilir.

**3. OUTPUT (Çıkış) — Elasticsearch'e Yazma**
```ruby
output {
  elasticsearch {
    hosts => ["elasticsearch:9200"]
    index => "logstash-%{+YYYY.MM.dd}"    ← Her gün yeni bir index oluşturur
  }
}
```
İşlenen loglar, Elasticsearch'in port `9200`'üne gönderilir. Index adı tarih bazlıdır: `logstash-2026.07.17` gibi. Bu sayede eski loglar gün bazında silinebilir.

#### Port & Bağlantı
- **Port 5044** (host'a açık + iç ağ) ← Filebeat buraya bağlanır
- **Elasticsearch'e bağlantı**: `elasticsearch:9200` (iç ağ)

#### Bellek Ayarı
```yaml
LS_JAVA_OPTS: "-Xms256m -Xmx256m"    ← Min ve max 256MB JVM heap
```

---

### 4.3 — Elasticsearch (Log Veritabanı & Arama Motoru)

#### Dosya
- [docker-compose.yml — elasticsearch servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L287-L308)

#### Ne Yapar?
Elasticsearch, Logstash'ten gelen işlenmiş logları **saklar ve aranabilir hale getirir**. Full-text arama motoru olarak çalışır — milyonlarca log satırı arasında milisaniyeler içinde arama yapabilir.

#### Port & Erişim
- **Port 9200** (host'a açık + iç ağ)
- REST API: `http://elasticsearch:9200`
- Logstash bu porta veri yazar
- Kibana bu porttan veri okur

#### Yapılandırma
```yaml
environment:
  - discovery.type=single-node          ← Tek node cluster (üretimde birden fazla node kullanılır)
  - xpack.security.enabled=false        ← Güvenlik kapalı (iç ağda çalıştığı için)
  - "ES_JAVA_OPTS=-Xms128m -Xmx128m"  ← JVM bellek limiti: 128MB
```

#### Veri Saklama
```yaml
volumes:
  - elk_data:/usr/share/elasticsearch/data    ← Docker named volume'da kalıcı olarak saklanır
```

#### İndex Yapısı
Logstash tarafından günlük index'ler oluşturulur:
```
logstash-2026.07.15
logstash-2026.07.16
logstash-2026.07.17
```
Her index o güne ait tüm container loglarını içerir.

---

### 4.4 — Kibana (Log Görselleştirme & Analiz Arayüzü)

#### Dosya
- [docker-compose.yml — kibana servisi](file:///home/omadali/dosyaubuntu/transendence/docker-compose.yml#L328-L345)

#### Ne Yapar?
Kibana, Elasticsearch'teki logları **web arayüzünden aranabilir, filtrelenebilir ve görselleştirilebilir** hale getirir. Log analizi, hata ayıklama ve pattern tespiti için kullanılır.

#### Port & Erişim
- **İç port 5601** (host'a da açık)
- Web arayüzü: `https://localhost:HTTPS_PORT/kibana` (Nginx üzerinden)
- `SERVER_BASEPATH=/kibana` → `/kibana` alt yolunda çalışır

#### Elasticsearch Bağlantısı
```yaml
ELASTICSEARCH_HOSTS: http://elasticsearch:9200    ← Docker iç ağ üzerinden ES'e bağlanır
```

#### Kibana'da Neler Yapılabilir?

| Özellik | Açıklama |
|---------|----------|
| **Discover** | Log satırlarını arama, filtreleme, zaman aralığı seçme |
| **Filtreleme** | `service_name: transcendence-backend` ile sadece backend loglarını görme |
| **Zaman bazlı analiz** | "Son 1 saatte kaç hata oldu?" gibi sorguları cevaplama |
| **Dashboard** | Log verilerinden grafikler oluşturma (hata trendi, istek dağılımı) |
| **KQL Sorguları** | `message: "error" AND service_name: "transcendence-nginx"` gibi sorgular |

---

## 🔵 BÖLÜM 5: ELK VERİ AKIŞI ÖZETİ

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          ELK VERİ AKIŞI                                │
│                                                                        │
│  Container Logları                                                     │
│  (/var/lib/docker/containers/*/*.log)                                  │
│         │                                                              │
│         ▼                                                              │
│  ┌─────────────┐   Docker Socket'ten metadata ekler                    │
│  │  Filebeat    │   (container adı, image, label'lar)                  │
│  └──────┬──────┘                                                       │
│         │ Beats protokolü, port 5044                                   │
│         ▼                                                              │
│  ┌─────────────┐   1. Input: Beats port 5044'ten alır                  │
│  │  Logstash    │   2. Filter: service_name alanı ekler                │
│  │  (Pipeline)  │   3. Output: Elasticsearch'e yazar                   │
│  └──────┬──────┘                                                       │
│         │ HTTP, port 9200                                              │
│         ▼                                                              │
│  ┌───────────────┐  Logları tarih bazlı index'lerde saklar             │
│  │ Elasticsearch  │  (logstash-2026.07.17)                             │
│  │ (Arama Motoru) │  Full-text arama yapılabilir                       │
│  └──────┬────────┘                                                     │
│         │ HTTP, port 9200                                              │
│         ▼                                                              │
│  ┌─────────────┐   Web arayüzünde logları arama, filtreleme            │
│  │   Kibana     │   Dashboard oluşturma, pattern analizi               │
│  │  (:5601)     │                                                      │
│  └──────┬──────┘                                                       │
│         │                                                              │
│         ▼                                                              │
│  Nginx (/kibana) → Kullanıcı Tarayıcı                                 │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔶 BÖLÜM 6: MONİTORİNG vs ELK KARŞILAŞTIRMASI

| Özellik | Monitoring Stack | ELK Stack |
|---------|-----------------|-----------|
| **Ne toplar?** | Sayısal metrikler (CPU %, istek/sn, bağlantı sayısı) | Metin logları (hata mesajları, istek detayları) |
| **Veri türü** | Zaman serisi (time series) | Yapılandırılmamış metin (unstructured text) |
| **Depolama** | Prometheus (TSDB) | Elasticsearch (inverted index) |
| **Görselleştirme** | Grafana | Kibana |
| **Sorgulama dili** | PromQL | KQL / Lucene |
| **Tipik soru** | "Son 1 saatte CPU %80'i aştı mı?" | "Backend'de NullReferenceException hatası nerede oluştu?" |
| **Profil** | `--profile monitoring` | `--profile elk` |
| **Veri toplayıcılar** | Node Exporter, cAdvisor, Postgres Exporter, Nginx Exporter | Filebeat |

---

## 🔶 BÖLÜM 7: TÜM PORT HARİTASI

| Servis | İç Port | Host Port | Kimler Bağlanır | Ne İçin |
|--------|---------|-----------|-----------------|---------|
| **Nginx** | 80 | `HTTP_PORT` | Kullanıcı tarayıcı | HTTP → HTTPS redirect |
| **Nginx** | 443 | `HTTPS_PORT` | Kullanıcı tarayıcı | Ana trafik |
| **Nginx** | 8888 | — | nginx-exporter | stub_status metrikleri |
| **Prometheus** | 9090 | 9090 | Grafana, Nginx, Tarayıcı | Metrik sorguları |
| **Grafana** | 3000 | — | Nginx (/grafana/) | Dashboard arayüzü |
| **Node Exporter** | 9100 | — | Prometheus | Host metrikleri |
| **cAdvisor** | 8080 | — | Prometheus | Container metrikleri |
| **Postgres Exporter** | 9187 | — | Prometheus | DB metrikleri |
| **Nginx Exporter** | 9113 | — | Prometheus | Nginx metrikleri |
| **Elasticsearch** | 9200 | 9200 | Logstash, Kibana | Log depolama/arama |
| **Logstash** | 5044 | 5044 | Filebeat | Log alma (Beats) |
| **Kibana** | 5601 | 5601 | Nginx (/kibana) | Log arayüzü |
| **PostgreSQL** | 5432 | `DB_PORT` | Backend, Postgres Exporter | Veritabanı |

---

## 🔶 BÖLÜM 8: HIZLI REFERANS — "KİM KİME BAĞLANIR?"

```
nginx-exporter ──► Nginx:8888/nginx_status ──► Prometheus:9090
postgres-exporter ──► PostgreSQL:5432 ──► Prometheus:9090
node-exporter ──► Host /proc, /sys ──► Prometheus:9090
cAdvisor ──► Docker Socket + /sys/fs/cgroup ──► Prometheus:9090
Prometheus:9090 ──► Grafana:3000 ──► Nginx:443/grafana/ ──► Tarayıcı

Filebeat ──► Docker Container Logları ──► Logstash:5044 ──► Elasticsearch:9200 ──► Kibana:5601 ──► Nginx:443/kibana ──► Tarayıcı
```
