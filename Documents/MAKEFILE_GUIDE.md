# 🚀 Transendence - Makefile Komutları

## Hızlı Referans

### 📦 Tüm Servisler
```bash
make up          # Tüm servisleri başlat
make down        # Tüm servisleri durdur
make logs        # Tüm logları göster
make rebuild     # Temizle ve yeniden build et
make clean       # Veritabanını da sil
```

---

## 🎨 Frontend Komutları (SvelteKit)

### Başlat & Build
```bash
make frontend-up       # Frontend + Database başlat
make frontend-build    # Frontend image build et
make frontend-rebuild  # Frontend'i sil ve yeniden build et
```

### Kontrol & Debug
```bash
make frontend-logs     # Frontend loglarını göster (-f: live follow)
make frontend-shell    # Frontend container'ında shell aç
make frontend-down     # Sadece Frontend'i durdur
```

### Örnek Workflow
```bash
# 1. Frontend'i başlat
make frontend-up

# 2. Loglarını takip et (ayrı terminal)
make frontend-logs

# 3. Hata varsa shell'e gir
make frontend-shell
npm install  # dependency eksikse
npm run build # build kontrol et
```

---

## ⚙️ Backend Komutları (.NET 8)

### Başlat & Build
```bash
make backend-up        # Backend + Database başlat
make backend-build     # Backend image build et
make backend-rebuild   # Backend'i sil ve yeniden build et
```

### Kontrol & Debug
```bash
make backend-logs      # Backend loglarını göster
make backend-shell     # Backend container'ında shell aç
make backend-down      # Sadece Backend'i durdur
```

### Örnek Workflow
```bash
# 1. Backend'i başlat
make backend-up

# 2. Loglarını takip et
make backend-logs

# 3. Database bağlantısını kontrol et
make backend-shell
# .NET CLI komutları kullanabilirsin
```

---

## 🗄️ Database Komutları (PostgreSQL)

### Başlat & Kontrol
```bash
make database-up       # Database başlat
make database-logs     # Database loglarını göster
make database-shell    # PostgreSQL psql'e gir (admin)
make database-down     # Database'i durdur
```

### SQL Örnekleri
```bash
# psql'de
make database-shell

# SQL komutları
\dt                    # Tabloları listele
SELECT * FROM users;   # Veri sor
\q                     # Çık
```

---

## 🔀 Nginx Reverse Proxy

```bash
make nginx-up          # Tüm servisleri Nginx ile başlat
make nginx-logs        # Nginx loglarını göster
make nginx-down        # Nginx'i durdur
```

---

## 📊 Loglar & Debug

### Service-Specific Loglar
```bash
make logs-frontend     # Sadece Frontend logları
make logs-backend      # Sadece Backend logları
make logs-nginx        # Sadece Nginx logları
make logs-db           # Sadece Database logları
```

### Shells
```bash
make shell-frontend    # Frontend shell
make shell-backend     # Backend shell
make shell-nginx       # Nginx shell
make shell-db          # PostgreSQL shell
```

---

## 🏥 Health Check

```bash
make test-health       # Tüm servisleri kontrol et
```

---

## 📋 Yaygın Senaryolar

### Senaryo 1: Frontend'i Bağımsız Test Et
```bash
make frontend-up
make frontend-logs
# http://localhost:3000 (docker'da)
# veya http://localhost:8080 (nginx'ten)
```

### Senaryo 2: Backend'i Bağımsız Test Et
```bash
make backend-up
make backend-logs
# http://localhost:5000
# veya http://localhost:8080/api (nginx'ten)
```

### Senaryo 3: Tüm Servisleri Çalıştır
```bash
make up
make logs

# Ayrı terminal
make test-health
```

### Senaryo 4: Build Hatası Var
```bash
# 1. Logları kontrol et
make frontend-logs

# 2. Container'a gir
make frontend-shell

# 3. Manual olarak build et ve hata kodu oku
npm run build

# 4. Yeniden build et
make frontend-rebuild
```

### Senaryo 5: Database Reset
```bash
make clean           # Veritabanı da silinir
make database-up     # Yeni database başlat
```

---

## 💡 İpuçları

- **Log Follow**: `make logs` veya `make frontend-logs` için `-f` flag otomatik eklenir (live update)
- **Multiple Terminals**: 
  - Terminal 1: `make frontend-up` + `make frontend-logs`
  - Terminal 2: `make backend-up` + `make backend-logs`
  - Terminal 3: `make test-health`
- **Hızlı Restart**: `make frontend-rebuild` tüm vesiteleri sıfırlar
- **Database Bağlantısı**: Backend/Frontend Database'e otomatik bağlanır

---

## 🔗 Erişim Noktaları

| Service | Port | URL |
|---------|------|-----|
| Frontend | 3000 | http://localhost:3000 |
| Backend | 5000 | http://localhost:5000 |
| Nginx | 8080 | http://localhost:8080 |
| Database | 5432 | localhost:5432 |

---

## ⚡ Kısayol Kombinasyonlar

```bash
# Frontend geliştir (2 terminal açık):
Terminal 1: make frontend-up && make frontend-logs
Terminal 2: make frontend-shell

# Backend geliştir (2 terminal açık):
Terminal 1: make backend-up && make backend-logs
Terminal 2: make backend-shell

# Full test:
make clean && make rebuild && make test-health
```
