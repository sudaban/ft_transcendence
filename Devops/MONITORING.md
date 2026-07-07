# Monitoring - Prometheus & Grafana

## Overview

| Service | Container | Port | Description |
|---|---|---|---|
| Prometheus | transendence-prometheus | 9090 | Metrics collection & alerting |
| Grafana | transendence-grafana | 3001 (direct) / 443 (via nginx) | Dashboards & visualization |
| Node Exporter | transendence-node-exporter | 9100 | Host/system metrics |
| cAdvisor | transendence-cadvisor | 8080 | Container metrics (CPU, mem, net) |
| Postgres Exporter | transendence-postgres-exporter | 9187 | PostgreSQL metrics |
| Nginx Exporter | transendence-nginx-exporter | 9113 | Nginx request/connection metrics |
| **Autoheal** | transendence-autoheal | — | Auto-restarts unhealthy containers |

## Auto-Heal

`willfarrell/autoheal` container monitors all containers with `autoheal=true` label every **30 seconds**. If a container becomes **unhealthy** (healthcheck fails 3 times in a row), autoheal automatically restarts it.

### How it works
1. Every container has a `healthcheck` (interval: 30s, retries: 3)
2. Docker marks container as `unhealthy` after 3 consecutive failures
3. Autoheal detects unhealthy containers and restarts them
4. Logs restart events: `docker compose logs -f autoheal`

### Healthcheck status
```bash
docker ps                           # STATUS column shows health
docker inspect --format='{{.State.Health.Status}}' <container_name>
```

## Access

| URL | Description |
|---|---|
| `https://localhost/grafana/` | Grafana (via HTTPS nginx proxy) |
| `http://localhost:9090` | Prometheus UI |
| `http://localhost:9090/targets` | Prometheus scrape targets status |

### Grafana Login
- **User**: `admin`
- **Password**: `transendence42`
- Can be changed via `.env`: `GRAFANA_ADMIN_USER` / `GRAFANA_ADMIN_PASSWORD`

## Make Commands

```bash
make monitoring-up      # Start only monitoring services
make monitoring-down    # Stop monitoring services
make monitoring-logs    # View monitoring logs (follow mode)
make test-health        # Health check all services incl. Prometheus & Grafana
```

## Logs

```bash
# All monitoring logs
docker compose logs --tail=20 prometheus grafana

# Single service log
docker compose logs -f prometheus
docker compose logs -f grafana
docker compose logs -f cadvisor
docker compose logs -f node-exporter
docker compose logs -f postgres-exporter
docker compose logs -f nginx-exporter
```

## Alert Rules

Defined in `monitoring/prometheus/alert_rules.yml`:

| Alert | Condition | Severity |
|---|---|---|
| InstanceDown | Any target down > 1min | critical |
| HighCpuUsage | Container CPU > 80% for 5min | warning |
| HighMemoryUsage | Container memory > 85% for 5min | warning |
| DatabaseDown | PostgreSQL unreachable > 1min | critical |
| DatabaseHighConnections | Active connections > 80 for 5min | warning |
| NginxHighErrorRate | 5xx rate > 5% for 5min | warning |
| DiskSpaceRunningLow | Disk usage > 85% for 5min | warning |

## Dashboard Panels

Pre-provisioned dashboard: **Transendence Monitoring**

- Container CPU & Memory usage
- Nginx HTTP request rate & active connections
- PostgreSQL active connections & database size
- System load average (gauge)
- Container network I/O (receive / transmit)
- Prometheus targets status (UP/DOWN)
- Active alerts list

## File Structure

```
monitoring/
├── prometheus/
│   ├── prometheus.yml         # Scrape config & targets
│   └── alert_rules.yml        # Alerting rules
└── grafana/
    ├── dashboards/
    │   └── transendence-dashboard.json  # Custom dashboard
    └── provisioning/
        ├── datasources/
        │   └── datasource.yml          # Prometheus datasource
        └── dashboards/
            └── dashboards.yml          # Dashboard auto-load config
```
