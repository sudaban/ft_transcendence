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
Ensure the following variables are securely populated (boot will fail if they are weak or missing):
- `JWT_SECRET_KEY` (Min 32 characters, required)
- `ADMIN_EMAIL` (Email for the seed admin account, required)
- `ADMIN_PASSWORD` (Password for the seed admin account, required)
- `DB_USER` & `DB_PASSWORD` (PostgreSQL credentials)
- `HTTP_PORT` (e.g., 8080 - HTTP bind port, required)
- `HTTPS_PORT` (e.g., 8443 - HTTPS bind port, required)
- `OAUTH_INTRA_CLIENT_ID` & `OAUTH_INTRA_CLIENT_SECRET` (42 API credentials)
- `OAUTH_GOOGLE_CLIENT_ID` & `OAUTH_GOOGLE_CLIENT_SECRET` (Google OAuth credentials)
- `GEMINI_API_KEY` (Gemini API key for AI assistant & content moderation)

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
The application will be accessible securely at **`https://localhost:8443`** (or whatever port you set for `HTTPS_PORT`).

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
Our relational database schema is managed via Entity Framework Core Code-First migrations with strict indexes, cascade deletions, and keys:

```
                  +-----------------------------------+
                  |               Users               |
                  +-----------------------------------+
                  | Id (PK, int)                      |<--------------------------+
                  | Username (varchar, unique)        |                           |
                  | Email (varchar, unique)           |                           |
                  | PasswordHash / PasswordSalt       |                           |
                  | ProfilePicture / Bio / IsOnline   |                           |
                  | TwoFactorEnabled / TwoFactorSecret|                           |
                  +-----------------------------------+                           |
                     |       |      |      |      |                               |
      +--------------+       |      |      |      +-----------------+             |
      |                      |      |      |                        |             |
      v                      v      v      v                        v             |
+------------+   +------------+  +-------+  +------------+   +------------+       |
|  Follows   |   | UserBlocks |  | Posts |  | ChatRooms  |   | ChatRoomMem|       |
+------------+   +------------+  +-------+  +------------+   +------------+       |
|FollowerId  |   |BlockerId   |  |Id (PK)|  |Id (PK, int)|   |Id (PK, int)|       |
|FollowingId |   |BlockedId   |  |Content|  |Name        |   |ChatRoomId  |       |
|CreatedAt   |   |CreatedAt   |  |UserId |  |IsGroup     |   |UserId (FK) |----->|
+------------+   +------------+  |ImgUrl |  |CreatedAt   |   |JoinedAt    |       |
                                 |Date   |  +------------+   +------------+       |
                                 +-------+         ^                              |
                                   ^   |           |                              |
                                   |   +-------+   |                              |
                                   |           |   |                              |
                                   |           v   |                              |
                                   |     +-----------+                            |
                                   |     | Comments  |                            |
                                   |     +-----------+                            |
                                   |     |Id (PK)    |                            |
                                   |     |Content    |                            |
                                   |     |PostId (FK)|                            |
                                   |     |UserId (FK)|----------------------------+
                                   |     |CreatedAt  |                            |
                                   |     +-----------+                            |
                                   |                                              |
      +----------------------------+-----------------+                            |
      |                            |                 |                            |
      v                            v                 v                            v
+------------+               +------------+    +------------+               +------------+
| PostLikes  |               | SavedPosts |    |  Messages  |<--------------|DeletedMsgs |
+------------+               +------------+    +------------+               +------------+
|PostId (FK) |               |PostId (FK) |    |Id (PK, int)|               |MsgId (FK)  |
|UserId (FK) |               |UserId (FK) |    |ChatRoom(FK)|               |UserId (FK) |
|CreatedAt   |               |SavedAt     |    |SenderId(FK)|               |DeletedAt   |
+------------+               +------------+    |Content /At |               +------------+
                                               +------------+
```

### Table Definitions:
- **Users:** Stores hashed credentials, TOTP metadata, and account status fields (`isBanned`, `isDeleted`).
- **Follows:** Many-to-many join table for managing the mutual friendship and follower graph.
- **UserBlocks:** Tracks block lists to prevent message exchanges and room creation.
- **Posts & Comments:** Houses microblogging content. Cascades automatically on user/post removal.
- **PostLikes & SavedPosts:** Maps interactive user signals on feed posts.
- **ChatRooms, ChatRoomMembers & Messages:** Powers SignalR workspace channels, private/group messaging history.
- **DeletedMessages:** Implements a soft-delete log per user for individual message clearings.

---

## 7. Claimed Modules & Point Calculation
We claim **19 points** in total, exceeding the 14-point mandatory threshold:

| Category | Module | Complexity | Points | Developer(s) |
| :--- | :--- | :---: | :---: | :--- |
| **Web** | Framework for FE & BE (SvelteKit + .NET 10) | Major | 2 | idkahram (FE), sdaban (BE) |
| **Web** | Real-time WebSockets (SignalR chat & status) | Major | 2 | saincesu, sdaban |
| **Web** | User Interaction (Direct chat, follows, posts) | Major | 2 | sdaban, saincesu |
| **User Management** | Standard Auth & User profiles | Major | 2 | asezgin, sdaban |
| **Devops** | ELK Log Infrastructure (Elasticsearch, Logstash, Kibana) | Major | 2 | omadali |
| **Devops** | Prometheus & Grafana Monitoring | Major | 2 | omadali |
| **Artificial Intelligence** | LLM System Interface (Gemini Assistant) | Major | 2 | asezgin, sdaban |
| **Web** | Database ORM (EF Core) | Minor | 1 | saincesu, asezgin |
| **Web** | File Upload System (Secure avatar and post media) | Minor | 1 | sdaban, saincesu |
| **User Management** | OAuth 2.0 Integration (Google & 42 Intra) | Minor | 1 | asezgin, sdaban |
| **User Management** | Complete 2FA TOTP | Minor | 1 | asezgin |
| **Artificial Intelligence** | Content Moderation AI (Gemini Safety Checks) | Minor | 1 | asezgin |
| **TOTAL** | | | **19 / 19** | |

---

## 8. Detailed Module Implementations & Justifications

### 1. Framework for FE & BE (SvelteKit + .NET 10)
- **Justification:** Chosen to segregate the concerns completely through Clean Architecture.
- **Implementation:** SvelteKit handles reactive routing, state stores, and dynamic components on the frontend. ASP.NET Core 10 delivers a high-speed, type-safe Web API with dependency injection controllers. Both are fully containerized under Docker.

### 2. Real-time WebSockets (SignalR)
- **Justification:** Standardizes asynchronous bidirectional event streams.
- **Implementation:** SignalR Hubs (`ChatHub`) handle real-time message broadcasting, direct DMs, online/offline presence updates, and streaming AI chunks.

### 3. User Interaction
- **Justification:** Essential social layer for building network graphs.
- **Implementation:** Custom follow controller mapping relationships, a SignalR-powered direct messaging window, and a full-featured microblogging feed where posts can be liked, commented on, or bookmarked.

### 4. Standard Auth & User Profiles
- **Justification:** Secure, role-based registration/login flows.
- **Implementation:** HMAC-SHA512 salted hashing protects credentials. Frontends query specific profiles where users update their avatars, bios, and passwords in secure areas.

### 5. ELK Log Infrastructure
- **Justification:** Enterprise-grade central log inspection.
- **Implementation:** Filebeat monitors container outputs, sending logs to Logstash which parses fields (e.g. `service_name`) and routes them to Elasticsearch indices for visualization in Kibana.

### 6. Prometheus & Grafana Monitoring
- **Justification:** Visual inspection of server/resource health.
- **Implementation:** Prometheus pulls system metrics from Node-exporter, Postgres-exporter, Nginx-exporter, and a custom Docker-exporter. Grafana provisions dashboard panels with alerting thresholds for memory, CPU, and network bottlenecks.

### 7. LLM System Interface (Gemini Assistant)
- **Justification:** Provides users with a companion AI inside the messaging interface.
- **Implementation:** Integrates the Gemini API. The backend consumes Gemini's streamed HTTP response as an async stream (`IAsyncEnumerable`) and relays each token chunk to clients in real time over the SignalR connection (`AiMessageChunk` events), while enforcing rate limits (5 requests/min per user).

### 8. Database ORM (EF Core)
- **Justification:** Simplifies entity management and structure syncs.
- **Implementation:** Entity Framework Core maps database entities to PostgreSQL using Code-First workflows. Includes index definitions and foreign key rules.

### 9. File Upload System
- **Justification:** Enables profile visual customization safely.
- **Implementation:** Secure controllers handle multi-part media upload. Checks file extensions, sizes, and formats. Stores items in a volume-mounted static files directory.

### 10. OAuth 2.0 Integration
- **Justification:** Reduces registration friction with third-party auth.
- **Implementation:** Connects to Google OAuth and 42 Intra APIs. Frontend exchanges tokens with redirect callbacks to backend endpoints, logging the user in or auto-registering.

### 11. Complete 2FA TOTP
- **Justification:** Critical security layer to protect against credential leaks.
- **Implementation:** Implements RFC 6238 TOTP logic. Provides QR setup codes, validates numeric inputs on login, and allows enabling/disabling from the account settings.

### 12. Content Moderation AI
- **Justification:** Automates social guidelines and block harmful messages.
- **Implementation:** Middleware triggers safety prompts to Gemini when creating posts or comments. If the content is rejected, the DB save is cancelled, returning a clean error payload explaining the block.

---

## 9. Implemented Features Details

| Feature | Primary Developer | Supporting Developer | Description |
|:---|:---:|:---:|:---|
| **Real-time DM & SignalR Hub** | saincesu | sdaban | Setup the WebSocket Hub with message delivery, notifications, and client state bindings. |
| **Authentication & TOTP 2FA** | asezgin | sdaban | SHA512 hashing, TOTP generator, validation endpoints, and security middlewares. |
| **Microblogging & Social Graph** | sdaban | saincesu | Post, comment, follow controllers, feed feeds, and PostgreSQL database queries. |
| **Svelte 5 Responsive UI** | idkahram | - | Built UI, Svelte runes (`$state`, `$effect`), dark/light mode adjustments, and GSAP micro-animations. |
| **Nginx, SSL & Containerization** | omadali | - | Docker orchestration, HTTPS self-signed Nginx proxy config, Multi-stage builds, container & host metrics via Prometheus exporters. |
| **Gemini LLM & AI Moderation** | asezgin | sdaban | Integrated Gemini API, SignalR chunk streaming, content safety filter hooks, and bad request exception handlers. |
| **Privacy Policy & Terms of Service** | idkahram | - | Dedicated `/privacy` and `/terms` pages with project-specific content, linked from the login, register, sidebar, and settings footers (mandatory compliance pages). |

---

## 10. Individual Contributions

### sdaban (Team Lead & Backend Developer):
- Orchestrated team objectives and task definitions.
- Coded core startup controllers, middleware filters, validation checks, and database entities.
- Implemented environment validations (verifying `HTTP_PORT`, `HTTPS_PORT`, and strong keys) to prevent boot on weak credentials.
- Resolved security gaps in repository layers and database context.
- **Challenges:** Keeping a Clean-Architecture layer split coherent across five developers. Overcome by enforcing strict layer boundaries, shared abstractions, and mandatory PR reviews before merges.

### saincesu (Backend Developer):
- Engineered backend repository and DbContext relations.
- Coded direct messaging SignalR Hubs and client event mapping.
- Resolved in-memory query bottlenecks (`GetAllAsync`) with tracked/non-tracked LINQ db queries.
- **Challenges:** SignalR message duplication and N+1 query blowups under concurrent users. Overcome with connection-group scoping and `AsNoTracking` projections that fetch only required fields.

### idkahram (Frontend Developer):
- Built the reactive Svelte 5 component design layout.
- Developed modular responsive interfaces for feed, direct chat, and admin panels.
- Designed visual micro-interactions and transitions with Tailwind and GSAP (guarded to prevent console warnings).
- **Challenges:** GSAP animations touching the DOM during hydration produced console warnings. Overcome by guarding all DOM access behind `onMount`/`$effect` so it runs only client-side after mount.

### omadali (DevOps & Release Manager):
- Configured multi-container Docker orchestration and secure Nginx configuration on port `8443`.
- Configured PID 1 tini daemon signals and resolved Alpine Rolldown build errors.
- Handled HTTPS reverse proxy routing for SvelteKit and SignalR WebSockets.
- **Challenges:** Portability across rootless and rootful Docker hosts plus Alpine Rolldown build errors. Overcome with multi-stage builds and environment-driven Docker-socket paths so the stack boots on different setups.

### asezgin (Database & Security Specialist):
- Developed the secure SHA-512 password hashing routines and TOTP 2FA algorithm services.
- Coded the backend integration for Gemini AI Service (async streamed reply relayed over SignalR, rate limits, and client error handling).
- Implemented the content moderation safety checks called during post/comment creation.
- Configured EF Core entity configurations, database indexes, and schema join tables.
- **Challenges:** RFC 6238 TOTP validation and safe cancellation of long-running Gemini streams under rate limits. Overcome with tested time-step tolerance and cancellation-aware `IAsyncEnumerable` streaming.

---

## 11. Resources & AI Usage
- **Resources:**
  - .NET 10 Documentation: https://learn.microsoft.com/en-us/dotnet/
  - Svelte 5 Runes: https://svelte.dev/docs/svelte/runes
  - Docker Compose Specification: https://docs.docker.com/compose/
- **AI Usage:**
  - **Code Generation:** Used AI to scaffold EF Core entity mappings, AI moderation filtering middleware, and Svelte 5 layout components.
  - **Debugging:** Leveraged AI for diagnosing Docker networking resolution issues and optimizing database-level asynchronous LINQ queries.
  - **Refactoring:** Used AI to rewrite dockerfiles into optimized, multi-stage minimal builds.
  - **Verification:** Used AI to scan front-end components for GSAP DOM errors and configure inactive client-side hooks.
