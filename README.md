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

In `Development`, the backend applies EF Core migrations during startup when it uses a relational database provider. This keeps an existing local Docker volume aligned with the current schema.

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

## Webhook Testing

The webhook endpoint validates `X-Signature` as an HMAC SHA-256 signature using `WEBHOOK_SIGNATURE_SECRET`.

Expected responses:

- `202 Accepted`: first valid delivery saved for background processing
- `200 OK`: duplicate `transaction_id` already received
- `400 Bad Request`: invalid JSON or payload validation error
- `401 Unauthorized`: missing or invalid signature

Example payload:

```json
{"transaction_id":"txn_manual_001","contract_id":"contract_manual_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}
```

PowerShell example:

```powershell
$secret = "dev-payhook-secret"
$payload = '{"transaction_id":"txn_manual_001","contract_id":"contract_manual_001","amount":10.50,"payment_date":"2026-09-01T00:00:00Z","status":"Paid"}'
$key = [Text.Encoding]::UTF8.GetBytes($secret)
$bytes = [Text.Encoding]::UTF8.GetBytes($payload)
$hmac = [Security.Cryptography.HMACSHA256]::new($key)
$signature = "sha256=" + (($hmac.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join "")
Invoke-WebRequest -Uri "http://localhost:5000/webhooks/payment" -Method Post -ContentType "application/json" -Body $payload -Headers @{ "X-Signature" = $signature }
```

Send the same payload again to validate idempotency. The second response should be `200 OK`.

You can also import `collections/payhook.postman_collection.json` into Postman. It includes valid, duplicate, invalid signature, invalid JSON, validation error, and query examples.

## Demo Environment

- Frontend: `https://payhook-frontend.vercel.app/`
- Backend health check: `https://payhook-backend.onrender.com/health`
- Backend Swagger UI: `https://payhook-backend.onrender.com/swagger`

The backend is hosted on Render's free tier, so the first request after a period of inactivity can take longer while the service wakes up.

## Deployment Notes

Render backend environment variables:

```text
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:10000
ConnectionStrings__DefaultConnection=<render-postgres-connection-string>
WebhookSecurity__SignatureSecret=<webhook-signature-secret>
DOTNET_USE_POLLING_FILE_WATCHER=true
DOTNET_hostBuilder__reloadConfigOnChange=false
PaymentProcessing__ProcessingDelay=00:00:02
PaymentProcessing__QueueCapacity=1000
PaymentProcessing__PendingScanInterval=00:00:10
PaymentProcessing__PendingBatchSize=50
```

Vercel frontend environment variables:

```text
PAYHOOK_API_BASE_URL=https://payhook-backend.onrender.com
```

Postman manual validation:

1. Import `collections/payhook.postman_collection.json`.
2. Set `base_url` to `https://payhook-backend.onrender.com`.
3. Set `signature_secret` to the same value configured in Render.
4. Run `Health Check`, `Webhook - Valid Payment`, `Webhook - Duplicate Payment`, and `Payments - List`.

## Background Processing

The API stores each accepted webhook in PostgreSQL before returning `202 Accepted`. New events are pushed to a bounded in-memory `Channel`, and a hosted worker processes them outside the request path after the configured delay.

PostgreSQL remains the source of truth: the worker also scans pending events periodically, so events saved before an application restart can still be processed.

For production, a durable external broker such as RabbitMQ or Azure Service Bus would be preferred. Those services provide stronger delivery guarantees, independent worker scaling, dead-letter handling, and better operational visibility. The in-memory channel keeps local development lightweight while still supporting asynchronous processing and backpressure.

## Frontend Dashboard

The dashboard lists received payments, supports filters by processing status and contract ID, and refreshes automatically every 5 seconds with TanStack Query polling. This is the current real-time behavior; it is polling, not WebSocket or SignalR.

The Next.js frontend calls its local `/api/payments` route, which proxies requests to the backend configured by `PAYHOOK_API_BASE_URL` to avoid browser CORS issues during local development.

## URLs

- Backend: `http://localhost:5000`
- Health check: `http://localhost:5000/health`
- Swagger UI: `http://localhost:5000/swagger`
- Frontend: `http://localhost:3000`
- pgAdmin: `http://localhost:5050`
