# Payhook

Payment webhook service with a .NET 8 backend, Next.js frontend, and local PostgreSQL through Docker Compose.

## Structure

```text
payhook/
  backend/
    src/
      Payhook.Api/  # Web API .NET 8
  frontend/         # Next.js + TypeScript
```

## Requirements

- Docker and Docker Compose
- .NET SDK 8
- Node.js 22 or later
- pnpm

## Local Setup

1. Copy the environment variables:

```bash
cp .env.example .env
```

2. Start the database, pgAdmin, and backend:

```bash
docker compose up --build
```

3. Run the frontend outside Docker Compose:

```bash
pnpm install
pnpm frontend:dev
```

## Quality

```bash
pnpm lint
pnpm lint:fix
pnpm format:check
pnpm format
```

```bash
dotnet test backend/tests/Payhook.Api.Tests
```

## Background Processing

The API stores each accepted webhook in PostgreSQL before returning `202 Accepted`. New events are pushed to a bounded in-memory `Channel`, and a hosted worker processes them outside the request path after the configured delay.

PostgreSQL remains the source of truth: the worker also scans pending events periodically, so events saved before an application restart can still be processed.

For production, a durable external broker such as RabbitMQ or Azure Service Bus would be preferred. Those services provide stronger delivery guarantees, independent worker scaling, dead-letter handling, and better operational visibility. The in-memory channel keeps this local assessment lightweight while still demonstrating asynchronous processing and backpressure.

## URLs

- Backend: `http://localhost:5000`
- Health check: `http://localhost:5000/health`
- Swagger UI: `http://localhost:5000/swagger`
- Frontend: `http://localhost:3000`
- pgAdmin: `http://localhost:5050`
