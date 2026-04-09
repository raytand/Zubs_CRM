<div align="center">

<img src="https://img.shields.io/badge/Zubs-CRM-4F46E5?style=for-the-badge&logoColor=white" alt="Zubs CRM" />

# Zubs CRM

**Dental Clinic Management System**

A full-stack web application for managing dental clinic operations — patients, doctors, appointments, treatments, payments, and more.

[![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React_18-61DAFB?style=flat-square&logo=react&logoColor=black)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis_7-DC382D?style=flat-square&logo=redis&logoColor=white)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white)](https://www.docker.com/)
[![Render](https://img.shields.io/badge/Deployed_on-Render-46E3B7?style=flat-square&logo=render&logoColor=white)](https://render.com/)

[Live Demo](https://zubs-crm.onrender.com) · [API Docs](https://zubs-api-31mb.onrender.com/swagger) · [Report Bug](https://github.com/raytand/Zubs_CRM/issues)

</div>

---

## Overview

Zubs CRM is a clinic management platform built for dental practices. It provides role-based access for admins, doctors, and secretaries, with full CRUD for patients, appointments, medical records, dental charts, treatment records, services, and payments.

---

## Features

- **Authentication** — JWT access tokens + refresh token rotation, automatic cleanup of expired tokens
- **Role-based access** — Admin, Doctor, Secretary roles with per-endpoint authorization
- **Patients** — full profile management with medical history, dental charts, and appointment history
- **Appointments** — schedule management with doctor/patient filtering and status tracking
- **Dental Charts** — per-patient tooth diagrams with treatment notes
- **Medical Records** — structured patient health history
- **Treatment Records** — linked to appointments, track procedures performed
- **Services** — clinic service catalog with unique code enforcement
- **Payments** — payment tracking per appointment
- **Audit Logs** — automatic logging of key actions (create, update, delete appointments)
- **Redis Caching** — cache-aside pattern on all read-heavy endpoints with smart invalidation
- **Calendar View** — FullCalendar-powered appointment scheduling UI

---

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 Web API |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / WebApi) |
| ORM | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL 16 |
| Caching | Redis 7 via StackExchange.Redis |
| Auth | JWT Bearer + Refresh Tokens |
| Mapping | AutoMapper |
| Testing | xUnit |

### Frontend
| Layer | Technology |
|---|---|
| Framework | React 18 |
| Routing | React Router v6 |
| HTTP | Axios |
| Calendar | FullCalendar |
| Build | Vite |
| Serving | Nginx |

### Infrastructure
| Tool | Usage |
|---|---|
| Docker + Compose | Local development and production containerization |
| Render | Cloud deployment (API + Frontend) |

---

## Architecture

```
Zubs_CRM/
├── Zubs_BE/
│   ├── Zubs.Domain/          # Entities, Enums — no dependencies
│   ├── Zubs.Application/     # DTOs, Interfaces, Services, AutoMapper profiles
│   ├── Zubs.Infrastructure/  # EF Core, Repositories, Redis CacheService, Migrations
│   ├── Zubs.WebApi/          # Controllers, Middleware, Program.cs
│   └── Zubs.Tests/           # xUnit tests (Auth, Services)
└── zubs_fe/
    ├── src/
    │   ├── pages/            # Patients, Appointments, Doctors, etc.
    │   ├── components/       # Navbar
    │   └── services/         # Axios API clients
    └── nginx.conf            # Reverse proxy config
```

### Caching Strategy

Redis cache-aside pattern is applied to all read-heavy services:

| Service | Keys | TTL |
|---|---|---|
| Patients | `patients:all`, `patients:{id}` | 5 min / 30 min |
| Doctors | `doctors:all`, `doctors:{id}` | 10 min / 30 min |
| Services | `services:all`, `services:{id}` | 30 min / 60 min |
| Appointments | `appointments:all`, `appointments:doctor:{id}`, `appointments:patient:{id}` | 2 min |
| Medical Records | `medicalrecords:patient:{id}` | 10 min |
| Dental Charts | `dentalchart:{id}`, `dentalchart:patient:{id}` | 15 min |
| Treatment Records | `treatmentrecords:{id}`, `treatmentrecords:appointment:{id}` | 15 min |
| Payments | ❌ not cached — financial data must always be fresh | — |
| Audit Logs | ❌ not cached — write-only | — |

On any write operation, all related keys are invalidated in parallel via `Task.WhenAll`.

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download) (for local development without Docker)

### Run with Docker (recommended)

```bash
git clone https://github.com/raytand/Zubs_CRM.git
cd Zubs_CRM

# copy and fill in environment variables
cp .env.example .env

# start everything
docker compose up --build
```

App will be available at:
- Frontend: http://localhost
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Local Development (faster iteration)

Run only the infrastructure in Docker, API and frontend locally:

```bash
# start DB + Redis only
docker compose -f docker-compose.dev.yml up db redis

# run API with hot reload
cd Zubs_BE/Zubs.WebApi
dotnet watch run

# run frontend
cd zubs_fe
npm install
npm run dev
```

---

## Environment Variables

Create a `.env` file in the root (see `.env.example`):

```env
POSTGRES_DB=DentalCrmDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password
JWT_KEY=your_jwt_secret_minimum_32_characters_long
```

> JWT key must be at least 32 characters for HS256 signing.

---

## API Overview

All endpoints (except auth) require a `Bearer` JWT token.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Login, returns access + refresh token |
| `POST` | `/api/auth/refresh` | Refresh access token |
| `GET` | `/api/patients` | List all patients |
| `GET` | `/api/patients/{id}` | Get patient by ID |
| `POST` | `/api/patients` | Create patient |
| `PUT` | `/api/patients/{id}` | Update patient |
| `DELETE` | `/api/patients/{id}` | Delete patient |
| `GET` | `/api/appointments` | List all appointments |
| `GET` | `/api/appointments/doctor/{id}` | Appointments by doctor |
| `GET` | `/api/appointments/patient/{id}` | Appointments by patient |
| `GET` | `/api/doctors` | List all doctors |
| `GET` | `/api/services` | List all services |
| `GET` | `/api/payments` | List all payments |
| `GET` | `/api/auditlogs` | List audit logs |

Full interactive docs available at `/swagger`.

---

## Roles

| Role | Access |
|---|---|
| `Admin` | Full access to all endpoints |
| `Doctor` | Patients, appointments, medical records, dental charts, treatment records |
| `Secretary` | Patients, appointments, payments, services |

---

## Running Tests

```bash
cd Zubs_BE
dotnet test
```

Tests cover JWT token generation/validation, AutoMapper profile mappings, and key service logic.

---

## Deployment

The app is deployed on [Render](https://render.com/):

- **API**: Render Web Service — builds from `Zubs_BE/Dockerfile`
- **Frontend**: Render Static Site / Web Service — builds from `zubs_fe/Dockerfile`, nginx proxies `/api/` to the API service
- **Database**: Render PostgreSQL
- **Cache**: Redis via environment variable — use [Upstash](https://upstash.com/) free tier for managed Redis on Render

A `KeepAliveService` background service pings the health endpoint every 10 minutes to prevent Render free-tier sleep.

---

## Contributing

1. Fork the repo
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push: `git push origin feature/your-feature`
5. Open a Pull Request

---

## License

MIT — see [LICENSE](LICENSE) for details.
