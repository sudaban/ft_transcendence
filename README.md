*This project has been created as part of the 42 curriculum by sdaban, saincesu, idkahram, asezgin, omadali.*

# 🛡️ ft_transcendence - Real-Time Social Platform

## 1. Description
**ft_transcendence** is a premium, real-time social networking and microblogging platform designed to facilitate secure user interactions, media sharing, and instant communication. Designed from scratch following Clean Architecture and Domain-Driven Design principles, the platform integrates advanced DevOps pipelines, real-time WebSockets, micro-interactions, and visual monitoring.

### Key Features:
- **Instant Communication:** Direct messaging (DMs) with real-time text delivery and typing indicators.
- **Microblogging Engine:** Users can create posts with text and media, comment on posts, like posts, and bookmark posts.
- **Social Graph:** A mutual follow system to manage friendship, view followers, and check real-time online/offline statuses.
- **Advanced Security:** HMAC-SHA512 password hashing, custom TOTP-based Two-Factor Authentication (2FA), and secure OAuth 2.0 logins.
- **Interactive UI:** Smooth page transitions, glassmorphic elements, and micro-animations driven by Svelte 5 and GSAP.

---

## 2. Instructions

### Prerequisites:
- **Docker** (v24.0.0 or later)
- **Docker Compose** (v2.20.0 or later)
- **Make** (optional, for helper commands)

### Environment Configuration:
Before starting, copy the `.env.example` to `.env` and fill in the required environment variables:
```bash
cp .env.example .env
```
Ensure the following variables are securely populated:
- `JWT_SECRET_KEY` (Min 32 characters)
- `ADMIN_EMAIL` (Email for the seed admin account)
- `ADMIN_PASSWORD` (Password for the seed admin account)
- `DB_USER` & `DB_PASSWORD` (PostgreSQL credentials)
- `OAUTH_INTRA_CLIENT_ID` & `OAUTH_INTRA_CLIENT_SECRET` (42 API credentials)
- `OAUTH_GOOGLE_CLIENT_ID` & `OAUTH_GOOGLE_CLIENT_SECRET` (Google OAuth credentials)

### Compilation and Execution:
To start the entire core stack (Frontend, Backend, Database, Nginx, Autoheal) under HTTPS:
```bash
docker compose up -d --build
```
Or use the Makefile shortcuts:
```bash
make build
make up
```
The application will be accessible securely at **`https://localhost`**.

To launch the monitoring profile (Prometheus, Grafana, exporters):
```bash
docker compose --profile monitoring up -d
```
To launch the log management profile (Elasticsearch, Logstash, Kibana, Filebeat):
```bash
docker compose --profile elk up -d
```

---

## 3. Team Information & Roles

| Member | Assigned Role | Key Responsibilities |
| :--- | :--- | :--- |
| **sdaban** | Team Lead & Backend Developer | Spearheaded project coordination, designed system-wide architecture, enforced environment security checks, and configured core DB contexts. |
| **saincesu** | Backend Developer | Developed persistence repositories, SignalR Hubs for live messages/online statuses, and optimized EF Core query performance. |
| **idkahram** | Frontend Developer | Created custom Svelte 5 UI components, Tailwind CSS styling, responsive page structures, and premium page animations. |
| **omadali** | DevOps & Release Manager | Configured Nginx SSL reverse proxy, multi-stage minimal Dockerfiles, compose network configurations, Tini PID 1 setups, and health checks. |
| **asezgin** | Database & Security Specialist | Implemented secure hashing routines, TOTP 2FA services, and configured DbContext relationships. |

---

## 4. Project Management
We adopted **Agile/Scrum** methodologies to organize our development sprints:
- **Task Organization & Deployment:** Managed features and deployment tasks using GitHub workflows, organizing work via **GitHub Pull Requests** and a structured **merging** workflow.
- **Communication & Meetings:** Used **WhatsApp** as our primary communication channel for scheduling, conducting meetings, writing progress reports, and day-to-day syncs.

---

## 5. Technical Stack & Justifications

### Frontend: SvelteKit (Svelte 5)
- *Why:* Svelte 5's compilation-based approach eliminates virtual DOM overhead. Its new **Runes** (`$state`, `$derived`, `$effect`) offer highly readable, granular reactivity, which is ideal for real-time dashboards.

### Backend: ASP.NET Core (.NET 10)
- *Why:* Unmatched performance, type safety, and robust dependency injection. SignalR provides native support for WebSockets with automatic fallback mechanisms.

### Database: PostgreSQL 16
- *Why:* Enterprise-grade Relational Database Management System (RDBMS) with support for ACID transactions, robust foreign keys, indexing, and high-performance concurrent reads.

### Reverse Proxy & SSL: Nginx
- *Why:* Lightweight, high-throughput proxy server that terminates SSL (HTTPS) and securely handles websocket routing.

### Observability: ELK Stack & Prometheus + Grafana
- *Why:* Prometheus pulls metrics from container exporters, while Grafana visualizes host, DB, and network performance. ELK manages structured logging across all containers for post-mortem diagnostics.

---

## 6. Database Schema
Our relational schema is managed by Entity Framework Core with strict constraints:

```
+------------------+         +------------------+         +------------------+
|      Users       |         |      Posts       |         |     Comments     |
+------------------+         +------------------+         +------------------+
| Id (PK, int)     |<---+    | Id (PK, int)     |<---+    | Id (PK, int)     |
| Username (unique)|    |    | Content (text)   |    |    | Content (text)   |
| Email (unique)   |    +--->| UserId (FK)      |    +--->| PostId (FK)      |
| PasswordHash     |         | ImageUrl (varchar)    +--->| UserId (FK)      |
| PasswordSalt     |         | CreatedAt (date) |         | CreatedAt (date) |
| ProfilePicture   |         +------------------+         +------------------+
| IsOnline / 2FA   |
+------------------+
    |          |
    |          +-------------------------+
    v                                    v
+------------------+             +------------------+
|     Follows      |             |    UserBlocks    |
+------------------+             +------------------+
| FollowerId (FK)  |             | BlockerId (FK)   |
| FollowingId (FK) |             | BlockedId (FK)   |
| CreatedAt (date) |             | CreatedAt (date) |
+------------------+             +------------------+
```

### Key Tables:
- **Users:** Stores credentials, TOTP secrets, status metadata, and roles.
- **Posts & Comments:** Linked via cascade delete constraints.
- **Follows:** Self-referencing N-M relationship mapping follower-following states.
- **UserBlocks:** Maps block relationships to dynamically restrict messaging and room creation.
- **ChatRooms, ChatRoomMembers & Messages:** Handles direct messaging groups and histories.

---

## 7. Claimed Modules & Point Calculation
We claim **16 points** in total, exceeding the 14-point threshold:

| Category | Module | Complexity | Points |
| :--- | :--- | :---: | :---: |
| **Web** | Framework for FE & BE (SvelteKit + .NET 10) | Major | 2 |
| **Web** | Real-time WebSockets (SignalR chat & status) | Major | 2 |
| **Web** | User Interaction (Direct chat, follows, posts) | Major | 2 |
| **User Management** | Standard Auth & User profiles | Major | 2 |
| **Devops** | ELK Log Infrastructure (Elasticsearch, Logstash, Kibana) | Major | 2 |
| **Devops** | Prometheus & Grafana Monitoring | Major | 2 |
| **Database** | Database ORM (EF Core) | Minor | 1 |
| **Web** | File Upload System (Secure avatar and post media) | Minor | 1 |
| **User Management** | OAuth 2.0 Integration (Google & 42 Intra) | Minor | 1 |
| **User Management** | Complete 2FA TOTP | Minor | 1 |
| **TOTAL** | | | **16 / 19** |

---

## 8. Individual Contributions

### sdaban:
- Orchestrated team objectives and task definitions.
- Coded core startup controllers, middleware filters, and database entities.
- Implemented environment validations to prevent boot on weak credentials.
- Resolved security gaps in custom repository layers.

### saincesu:
- Engineered backend repository and DbContext relations.
- Coded direct messaging SignalR Hubs and client event mapping.
- Resolved in-memory query bottlenecks (`GetAllAsync`) with tracked/non-tracked LINQ db queries.

### idkahram:
- Built the reactive Svelte 5 component design layout.
- Developed modular responsive interfaces for feed, direct chat, and admin panels.
- Designed visual micro-interactions and transitions with Tailwind and GSAP.

### omadali:
- Configured multi-container Docker orchestration and secure Nginx configuration.
- Configured PID 1 tini daemon signals and resolved Alpine Rolldown build errors.
- Handled HTTPS reverse proxy routing for SvelteKit and SignalR WebSockets.

### asezgin:
- Developed the secure SHA-512 password hashing routines.
- Built custom Two-Factor Authentication TOTP algorithm services and QR code generator integrations.
- Configured EF Core entity configurations, indexes, and database schemas.

---

## 9. Resources & AI Usage
- **Resources:**
  - .NET 10 Documentation: https://learn.microsoft.com/en-us/dotnet/
  - Svelte 5 Runes: https://svelte.dev/docs/svelte/runes
  - Docker Compose Specification: https://docs.docker.com/compose/
- **AI Usage:**
  - **Code Generation:** Used AI to scaffold boilerplate EF Core entity mappings and Svelte 5 layout components.
  - **Debugging:** Leveraged AI for diagnosing Docker networking resolution issues and optimizing database-level asynchronous LINQ queries.
  - **Refactoring:** Used AI to rewrite dockerfiles into optimized, multi-stage minimal builds.
