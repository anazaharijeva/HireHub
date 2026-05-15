# HireHub

**HireHub – Microservices-Based Recruitment Platform**  
Designed and implemented a distributed job recruitment system using ASP.NET Core microservices architecture with RabbitMQ event-driven messaging, JWT authentication, and SQL Server database-per-service design. Built API Gateway routing (Ocelot), integration events, and modular services for jobs, applications, messaging, user profiles, and notifications. Containerized the stack using Docker Compose following clean architecture (API / Application / Domain / Infrastructure) with DTOs, FluentValidation, AutoMapper (Job service), MediatR (Application service), repository-style data access, and RabbitMQ publishing from services plus a hosted consumer in Notification.

## Repository layout

| Path | Description |
|------|-------------|
| `ApiGateway/` | Ocelot API Gateway (`/api/...` → services) |
| `src/BuildingBlocks/` | `HireHub.Contracts` (integration events, `IIntegrationEventPublisher`), `HireHub.EventBus` (RabbitMQ publisher), `HireHub.ApiCommon` (shared JWT validation) |
| `src/Services/AuthService/` | Registration, login, JWT + refresh tokens, BCrypt, roles, `UserRegisteredEvent` |
| `src/Services/UserService/` | Candidate & recruiter profiles, search |
| `src/Services/JobService/` | Jobs, categories, filters, pagination, `JobCreatedEvent` |
| `src/Services/ApplicationService/` | Apply, status updates, withdraw, duplicate guard, MediatR, `ApplicationCreatedEvent` / `ApplicationUpdatedEvent` |
| `src/Services/MessagingService/` | Conversations & messages, `MessageSentEvent` |
| `src/Services/NotificationService/` | In-app notifications + RabbitMQ `IntegrationEventsWorker` |
| `Frontend/` | React + TypeScript (Vite) minimal UI hitting the gateway |
| `docker/` | Dockerfiles per service + frontend |
| `docker-compose.yml` | SQL Server, RabbitMQ, all APIs, gateway, frontend |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for `Frontend/`)
- SQL Server (local or Docker) and RabbitMQ for full event flow

## Configuration

- Each API uses `appsettings.json` with `ConnectionStrings:*`, shared `Jwt` signing settings (must match Auth for validation), and `RabbitMq` for the event bus.
- Frontend: copy `Frontend/.env.example` to `Frontend/.env` and set `VITE_API_BASE_URL` (default `http://localhost:5000` for the gateway).

## Run locally (without Docker)

1. Start **SQL Server** and **RabbitMQ** (defaults in appsettings assume `localhost` and password `Your_password123` for `sa`—change to match your instance).
2. From the repo root:

```powershell
dotnet run --project src/Services/AuthService/AuthService.Api/AuthService.Api.csproj --urls "http://localhost:5001"
dotnet run --project src/Services/UserService/UserService.Api/UserService.Api.csproj --urls "http://localhost:5002"
dotnet run --project src/Services/JobService/JobService.Api/JobService.Api.csproj --urls "http://localhost:5003"
dotnet run --project src/Services/ApplicationService/ApplicationService.Api/ApplicationService.Api.csproj --urls "http://localhost:5004"
dotnet run --project src/Services/MessagingService/MessagingService.Api/MessagingService.Api.csproj --urls "http://localhost:5005"
dotnet run --project src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj --urls "http://localhost:5006"
dotnet run --project ApiGateway/HireHub.ApiGateway.csproj --urls "http://localhost:5000"
```

Update `ApiGateway/ocelot.json` downstream hosts/ports to match your local URLs, or use Docker Compose where hostnames are service names.

3. Frontend:

```powershell
cd Frontend
npm install
npm run dev
```

## Docker Compose

```powershell
docker compose up --build
```

- Gateway: `http://localhost:5000`
- RabbitMQ management UI: `http://localhost:15672` (guest/guest)
- SQL Server: `localhost,1433` (sa / `Your_password123`)
- Frontend: `http://localhost:3000` (built static site; API calls go to `http://localhost:5000` per build arg)

First start may take a minute while SQL Server becomes ready; notification consumer retries RabbitMQ until it connects.

## Security notes

- Replace `Jwt:SigningKey` and SQL `sa` password for any real deployment.
- AutoMapper 12.0.1 reports a NuGet advisory; pin to a patched version when available.

## Optional extensions (CV “+” ideas)

- Azure / Kubernetes / GitHub Actions CI/CD  
- SignalR hub for live messaging  
- xUnit + WebApplicationFactory integration tests per service  
- Elasticsearch / Redis  

## License

MIT (adjust as needed).
