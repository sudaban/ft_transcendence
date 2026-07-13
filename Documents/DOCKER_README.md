# transcendence - Docker Setup

Local development environment using Docker Compose with SvelteKit Frontend, .NET Backend, PostgreSQL Database, and Nginx Reverse Proxy.

## Gereksinimler / Requirements

- Docker (v20.10+)
- Docker Compose (v2.0+)

## Hızlı Başlangıç / Quick Start

### 1. Environment Dosyası Oluştur / Create Environment File
```bash
cp .env.example .env
```

### 2. Docker'ı Başlat / Start Docker
```bash
docker-compose up -d
```

### 3. Servislere Erişim / Access Services

| Service | URL | Port |
|---------|-----|------|
| Nginx (Reverse Proxy) | http://localhost:8080 | 8080 |
| Frontend (SvelteKit) | http://localhost:8080 | 8080 |
| Backend API | http://localhost:8080/api | 8080 |
| Database | localhost | 5432 |

## Komutlar / Commands

### Tüm servisleri başlat / Start all services
```bash
docker-compose up
```

### Arka planda çalıştır / Run in background
```bash
docker-compose up -d
```

### Logları görüntüle / View logs
```bash
docker-compose logs -f
```

### Belirli bir serverin loglarını görmek / View specific service logs
```bash
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f nginx
docker-compose logs -f database
```

### Servisleri durdur / Stop services
```bash
docker-compose down
```

### Veritabanını temizleyerek baştan başla / Stop and remove volumes
```bash
docker-compose down -v
```

### Konteynerleri rebuild et / Rebuild containers
```bash
docker-compose up --build
```

## Geliştirme / Development

### Frontend (SvelteKit)
- **Port**: 3000 (Nginx'ten 8080 üzerinden erişilebilir)
- **Build**: `npm run build` - SvelteKit build'i oluşturur
- **Run**: `node build/index.js` - Sunucuyu 3000 portunda başlatır
- Tailwind CSS entegre

### Backend (.NET 8)
- **Port**: 5000 (Nginx'ten /api üzerinden erişilebilir)
- **Database**: PostgreSQL
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core

### Nginx Reverse Proxy
- **Port**: 8080
- WebSocket desteği: `Connection` ve `Upgrade` headerları açık
- Frontend → Port 3000
- Backend API → Port 5000 (`/api/*` yönlendirilir)
- WebSocket → Backend'e yönlendirilir (`/ws`)

### Database Erişimi / Access Database
```bash
docker-compose exec database psql -U postgres -d transcendence
```

### Backend Console'a Erişim / Access Backend Container
```bash
docker-compose exec backend sh
```

### Frontend Console'a Erişim / Access Frontend Container
```bash
docker-compose exec frontend sh
```

### Nginx Console'a Erişim / Access Nginx Container
```bash
docker-compose exec nginx sh
```

## Sorun Giderme / Troubleshooting

### Port zaten kullanılıyorsa / If port is already in use
`.env` dosyasında port numaralarını değiştirin:
```
DB_PORT=5433
NGINX_PORT=8081
```

### Database bağlantı sorunu / Database connection error
```bash
docker-compose down -v
docker-compose up --build
```

### Frontend Build hatası / Frontend build error
```bash
docker-compose exec frontend npm install
docker-compose up --build frontend
```

### Backend bağlantı problemi / Backend connection issues
Logs kontrol edin:
```bash
docker-compose logs -f backend
```

## Proje Yapısı / Project Structure

```
transcendence/
├── Frontend/                    # SvelteKit + Tailwind CSS
│   ├── Dockerfile              # Node 20 Alpine multi-stage
│   ├── package.json
│   ├── svelte.config.js
│   ├── tailwind.config.js
│   ├── postcss.config.js
│   ├── src/
│   │   ├── routes/
│   │   │   ├── +layout.svelte
│   │   │   └── +page.svelte
│   │   └── styles/
│   │       └── app.css
│   └── .dockerignore
├── Backend/                     # ASP.NET Core 8.0
│   ├── Dockerfile              # .NET 8 Alpine multi-stage
│   ├── Backend.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   └── .dockerignore
├── nginx/                       # Reverse Proxy
│   ├── Dockerfile
│   └── nginx.conf              # WebSocket desteği ile konfigürasyon
├── Database/                    # PostgreSQL (Docker image)
├── docker-compose.yml           # Orchestration
├── .env.example                 # Configuration template
├── DOCKER_README.md             # Bu dosya
├── Makefile                     # Yardımcı komutlar
├── start.sh                     # Linux/Mac başlangıç scripti
└── start.bat                    # Windows başlangıç scripti
```

## Network

Tüm servisler `transcendence-network` ağında bağlıdır:
- Frontend (SvelteKit) → `http://frontend:3000`
- Backend (ASP.NET) → `http://backend:5000`
- Database (PostgreSQL) → `database:5432`

**Dış erişim**:
- Nginx (80 port'ta) → `http://localhost:8080`

## Environment Variables

### Frontend
```
PUBLIC_API_URL=http://localhost:8080/api
```

### Backend (.NET)
```
ConnectionStrings__DefaultConnection=Host=database;Port=5432;Database=transcendence;Username=postgres;Password=postgres
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Development
```

### Database
```
DB_USER=postgres
DB_PASSWORD=postgres
DB_NAME=transcendence
```

## WebSocket Konfigürasyonu

Nginx konfigürasyonu WebSocket desteği için hazırlanmıştır:
```nginx
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection "upgrade";
```

Backend'de WebSocket kullanmak için `/ws` endpoint'i kullanın.

## Performance İpuçları

1. **Frontend**: SvelteKit production build optimize edilmiştir
2. **Backend**: .NET 8 Alpine image küçük ve hızlıdır
3. **Database**: PostgreSQL 16 Alpine container
4. **Nginx**: Alpine Linux tabanlı, hafif

## Makefile Komutları

```bash
make up                 # Servisleri başlat
make down              # Servisleri durdur
make build             # Image'ları oluştur
make rebuild           # Temizle ve yeniden oluştur
make logs              # Logları göster
make shell-backend     # Backend shell'e gir
make shell-frontend    # Frontend shell'e gir
make shell-db          # Database shell'e gir
```

