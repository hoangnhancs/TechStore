# TechStore — Microservices E-Commerce Backend

A production-grade e-commerce backend built with .NET 9 microservices architecture, featuring distributed saga orchestration, multi-gateway payment processing, real-time notifications, and AI-powered recommendations.

---

## Architecture Overview

```
                        ┌──────────────────────────────────────┐
                        │           Client (Browser)           │
                        └──────────────────┬───────────────────┘
                                           │ HTTP / WebSocket
                        ┌──────────────────▼───────────────────┐
                        │           Gateway Service            │
                        │     (YARP Reverse Proxy + JWT)       │
                        └──────┬──────────────────────┬────────┘
                               │ REST                 │ REST
           ┌───────────────────▼──────┐   ┌───────────▼──────────────┐
           │     Identity Service     │   │      Product Service     │
           │  (Auth, JWT, gRPC Server)│   │  (Catalog, Stock, gRPC)  │
           └──────────────────────────┘   └──────────────────────────┘
                               │
                               │ REST / gRPC
           ┌────────────────────────────────────────────────────────────┐
           │                    Order Service                           │
           │          (Saga State Machine + Quartz Scheduler)           │
           └──────────┬─────────────────────────────────┬──────────────┘
                      │ Publish Events                   │
           ┌──────────▼───────────┐        ┌────────────▼─────────────┐
           │     RabbitMQ Bus     │        │      Payment Service     │
           │    (MassTransit)     │        │ (Stripe / Momo / VNPay)  │
           └──┬────────┬─────────┘        └──────────────────────────┘
              │        │
  ┌───────────▼──┐  ┌──▼────────────┐  ┌─────────────┐  ┌───────────────┐
  │Notification  │  │  Search Svc   │  │ Review Svc  │  │ Comment Svc   │
  │Svc (Email +  │  │  (MongoDB)    │  │ (SignalR)   │  │ (SignalR)     │
  │  SignalR)    │  └───────────────┘  └─────────────┘  └───────────────┘
  └──────────────┘
```

---

## Microservices

| Service | Responsibility | Key Tech |
|---------|---------------|----------|
| **GatewayService** | Reverse proxy, JWT validation, routing | YARP |
| **IdentityService** | Authentication, user management, JWT issuing | ASP.NET Identity, gRPC |
| **ProductService** | Product catalog, inventory, stock reservation | EF Core, gRPC |
| **OrderService** | Order lifecycle, saga orchestration | MassTransit Saga, Quartz |
| **PaymentService** | Multi-gateway payment processing | Stripe, Momo, VNPay |
| **CartService** | Shopping cart, real-time price sync | gRPC, EF Core |
| **NotificationService** | Email & in-app notifications | SignalR, Handlebars, MassTransit |
| **ReviewService** | Product reviews, ratings | SignalR, EF Core |
| **CommentService** | Product discussions | SignalR, EF Core |
| **SearchService** | Full-text product search | MongoDB |
| **RecommendationService** | AI product recommendations | gRPC, pgvector |
| **PhotoService** | Image upload & CDN management | Cloudinary |
| **EmailService** | Email template rendering & sending | Handlebars.NET |
| **VectorService** | ML embedding generation (Python) | FastAPI, sentence-transformers |
| **BuildingBlocks** | Shared DDD base classes, generic repos | EF Core, Repository/UoW patterns |
| **Contract** | Shared MassTransit message contracts | — |

---

## Tech Stack

### Core
- **.NET 9 / C#** — all backend services
- **ASP.NET Core** — REST APIs
- **Entity Framework Core 9** — ORM with code-first migrations
- **PostgreSQL** — primary database for all .NET services
- **MongoDB** — document store for SearchService full-text search

### Messaging & Event-Driven
- **RabbitMQ** — message broker (hosted on CloudAMQP)
- **MassTransit 8.5** — pub/sub, saga orchestration, outbox pattern
- **Transactional Outbox** — guarantees at-least-once event delivery

### Inter-Service Communication
- **gRPC** — synchronous service-to-service calls (Identity ↔ Order, Product ↔ Cart, etc.)
- **Protocol Buffers** — efficient binary serialization

### Real-Time
- **SignalR** — WebSocket-based live updates (orders, payments, reviews, comments)
- **Redis (StackExchange)** — SignalR scale-out backplane

### Scheduling
- **Quartz.NET** — persistent job scheduling for payment expiry timeouts

### Authentication
- **ASP.NET Core Identity** — user management, password hashing
- **JWT (HTTP-only cookies)** — stateless authentication
- **Role-based authorization** — Admin, Customer, etc.

### Payments
- **Stripe** — credit/debit card processing with webhook support
- **Momo** — Vietnamese e-wallet
- **VNPay** — Vietnamese bank transfer
- **Bank Transfer + COD** — manual payment methods

### AI / ML
- **Python FastAPI** — microservice for generating vector embeddings
- **sentence-transformers** — 384-dim semantic embeddings for product similarity
- **pgvector** — cosine similarity search in PostgreSQL

### Infrastructure
- **YARP** — API gateway with JWT verification and reverse proxy routing
- **Cloudinary** — cloud image storage and CDN
- **Mailpit** — local SMTP mock for development
- **Resend** — transactional email provider for production
- **Docker** — containerized local development

### Libraries
- **AutoMapper 14** — entity/DTO mapping
- **FluentValidation 12** — request validation
- **Handlebars.NET** — HTML email template rendering
- **Newtonsoft.Json** — JSON serialization

---

## Key Features

### Distributed Order Saga

The heart of the system is a `MassTransitStateMachine` orchestrating the entire order lifecycle across multiple services using the Saga pattern with automatic compensation on failure.

**Online Payment Flow**
```
OrderCreated → [Stock Reserved] → CreatePayment → [Payment Intent Created]
  → Start 5-min expiry timer → Customer pays (Stripe webhook)
  → [PaymentCompleted] → ConfirmOrder → [OrderConfirmed] → CommitStock → ✅ Done

Failure paths:
  StockReservationFailed      → CancelOrder
  PaymentFailed               → CancelOrder + ReleaseStock
  Payment timeout (5 min)     → CancelOrder + ReleaseStock
```

**COD (Cash on Delivery) Flow**
```
OrderCreated → [Stock Reserved] → SetWaitingForConfirmation
  → Start expiry timer (configurable) → Admin confirms → CreatePayment
  → [PaymentCreated] → ConfirmOrder → CommitStock → ✅ Done

Failure: PaymentFailed → back to WaitingForConfirmation (admin retries)
         Expiry timeout → CancelOrder + ReleaseStock
```

### Multi-Payment Gateway

Payment service uses a **Factory pattern** to route to the correct provider at runtime:

```
POST /api/payments/create
  → PaymentServiceFactory.GetService(paymentMethod)
  → [StripeService | MomoService | VNPayService | BankTransferService]
  → Webhook handler confirms payment async → publishes PaymentCompleted event
```

### Inventory Management

Stock is managed in two phases to prevent overselling:

1. **Reserve** — deducted from available stock at order creation
2. **Commit** — moved to sold stock after payment confirmation
3. **Release** — returned to available stock on cancellation

All stock operations use atomic database transactions with row-level locking.

### Real-Time Notifications

```
OrderConfirmed event (RabbitMQ)
  → NotificationService consumer
  → Send HTML email (Handlebars template)
  → Create in-app notification record
  → Push via SignalR to user + admin group
```

Email templates: `OrderCreated`, `OrderConfirmation`, `OrderCancelled`, `OrderStatusUpdate`

### AI-Powered Recommendations

```
Product viewed / purchased
  → UserActionTracking (PostgreSQL)
  → RecommendationService gRPC call
  → VectorService (Python FastAPI) generates 384-dim embedding
  → Stored in pgvector
  → Cosine similarity query returns top-N related products
```

### CQRS with MediatR

All business logic is separated into Commands (writes) and Queries (reads) handled via MediatR pipeline:

```
Controller → IRequest<T> (Command/Query)
  → MediatR Pipeline (validation, logging)
  → IRequestHandler<TRequest, TResponse>
  → Repository/UnitOfWork
```

---

## Project Structure

```
server/
├── BuildingBlocks/
│   ├── Shared.Core.EF/         # BaseEntity, AggregateRoot, IRepository, IUnitOfWork
│   ├── Infrastructure.EF/      # Generic EF Core repository + unit of work
│   ├── Shared.Web/             # JWT middleware, error handling, DI extensions
│   └── Infrastructure.Mongo/   # MongoDB base repository
├── Contract/                   # Shared MassTransit message contracts
│   └── Order/                  # OrderCreated, OrderConfirmed, OrderCancelled, ...
├── GatewayService/
├── IdentityService/
├── ProductService/
├── OrderService/
│   ├── Saga/                   # OrderSagaStateMachine + OrderSagaState
│   ├── Consumers/              # MassTransit consumers (CancelOrder, ConfirmOrder, ...)
│   └── Services/               # CQRS handlers (MediatR)
├── PaymentService/
│   └── Services/               # StripeService, MomoService, VNPayService, ...
├── CartService/
├── NotificationService/
│   ├── Consumers/              # OrderConfirmedConsumer, OrderCancelledConsumer, ...
│   ├── Services/               # HandleOrderConfirmedHandler, ...
│   └── Templates/ (EmailService) # OrderConfirmation.html, OrderCancelled.html, ...
├── ReviewService/
├── CommentService/
├── SearchService/
├── RecommendationService/
├── PhotoService/
├── EmailService/
│   └── Templates/              # Handlebars HTML email templates
└── VectorService/              # Python FastAPI ML service
```

---

## Architecture Patterns

| Pattern | Where Used |
|---------|-----------|
| **Microservices** | 14 independent .NET services + 1 Python service |
| **Saga Orchestration** | OrderSagaStateMachine (MassTransit) |
| **CQRS** | MediatR Commands + Queries in every service |
| **Domain-Driven Design** | AggregateRoot, Value Objects, Domain Events |
| **Repository + Unit of Work** | All services via BuildingBlocks abstractions |
| **Transactional Outbox** | MassTransit + EF Core — guaranteed event delivery |
| **API Gateway** | YARP — single entry point, JWT verification |
| **Factory** | PaymentServiceFactory — runtime provider selection |
| **Event-Driven** | All inter-service state changes via RabbitMQ events |
| **gRPC for sync calls** | Identity, Product, Recommendation inter-service calls |

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL
- RabbitMQ (or CloudAMQP account)
- Redis
- Python 3.10+ (for VectorService)

### Configuration

Each service uses `appsettings.json`. Key values to configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "RabbitMq": {
    "ConnectionString": "amqps://..."
  },
  "JwtOptions": {
    "SecretKey": "...",
    "Issuer": "...",
    "Audience": "..."
  },
  "Stripe": {
    "SecretKey": "sk_...",
    "WebhookSecret": "whsec_..."
  },
  "GrpcIdentity": "https://localhost:5001"
}
```

### Run Services

```bash
# Start infrastructure
docker compose up -d

# Run each service (example)
cd server/IdentityService && dotnet run
cd server/ProductService  && dotnet run
cd server/OrderService    && dotnet run
cd server/PaymentService  && dotnet run
# ... remaining services

# VectorService (Python)
cd server/VectorService
pip install -r requirements.txt
uvicorn main:app --port 8001
```

Database migrations are applied automatically on startup.

---

## Notable Implementation Highlights

- **Quartz + MassTransit integration** — payment windows are enforced via persistent Quartz jobs; the job fires an `OrderPaymentExpired` event consumed by the Saga, which then cancels the order and releases stock atomically.

- **Dual payment timer** — Online orders start the countdown only after `PaymentCreated` (not at order creation), avoiding false timeouts during payment intent setup. COD orders start the timer immediately after stock reservation to handle users who never confirm.

- **COD retry loop** — If payment record creation fails on a COD order, the Saga transitions back to `WaitingForConfirmation` instead of cancelling, allowing admin to retry rather than penalising the customer.

- **gRPC user sync** — `NotificationService` and other consumers maintain a local `UserInformation` cache synced from `IdentityService` via gRPC on startup, avoiding cross-service DB calls on every notification.

- **Outbox pattern** — all MassTransit publishes go through an EF Core outbox table in the same transaction as the DB write. A background dispatcher reads and publishes them, ensuring no events are lost if a service crashes after saving but before publishing.
