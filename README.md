# TechStore — Microservices E-Commerce Backend

[![Live Demo](https://img.shields.io/badge/Live-Demo-brightgreen?style=for-the-badge&logo=vercel)](https://your-live-demo-link.com)
[![Swagger API Docs](https://img.shields.io/badge/API-Swagger%20UI-blue?style=for-the-badge&logo=swagger)](https://your-swagger-link.com)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-blueviolet?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-%20-orange?style=for-the-badge&logo=rabbitmq)](https://www.rabbitmq.com/)
[![FastAPI](https://img.shields.io/badge/FastAPI-%20-009688?style=for-the-badge&logo=fastapi)](https://fastapi.tiangolo.com/)

A production-grade, highly scalable e-commerce backend built with a **.NET 9 microservices architecture**. This project serves as an advanced technical case study demonstrating distributed consistency, high-concurrency reservation systems, real-time notifications, and low-latency AI-powered search & recommendation pipelines.

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

## Key Architectural & Design Decisions

### 1. API Gateway Pattern (YARP)
Instead of exposing individual services to the public internet, a reverse proxy built with **YARP (Yet Another Reverse Proxy)** acts as the single entry point.
- **Why?** Centralizes CORS policies, SSL termination, and JWT authentication token verification. This shields internal services from authentication boilerplate and allows security policies to be updated in one place.

### 2. gRPC (Synchronous Queries) vs. RabbitMQ (Asynchronous Operations)
To achieve low latency and eventual consistency, a hybrid communication model is utilized:
- **gRPC (HTTP/2 with Protocol Buffers)**: Used for synchronous, read-heavy queries between services where real-time responses are required. (e.g., `CartService` retrieving live product listings, or `SearchService` retrieving recommendations). This reduces payload size and TCP socket overhead compared to HTTP/REST.
- **RabbitMQ + MassTransit (AMQP)**: Used for asynchronous, write-heavy state transitions where decoupling is critical. (e.g., placing an order triggers stock reservation, payment setup, and email alerts). If the payment service is down, order placement remains unaffected, and events are processed once the service recovers.

### 3. Saga Orchestration over Choreography
For complex distributed transactions like order fulfillment, **Saga Orchestration** (`MassTransitStateMachine` located in the `OrderService`) was chosen:
- **Why?** In choreography, services must react to each other's events directly, creating complex cyclic dependencies. Orchestration centralizes the control flow. The `OrderService` acts as a coordinator, making it easy to track the order's state, audit transitions, and trigger automated compensating transactions (such as rolling back inventory reservations) if a payment fails.

---

## Deep-Dive Case Studies & Solved Challenges

### Case Study 1: The Dual-Write Problem & Transactional Outbox
* **Problem**: When a user creates an order, the system must write to the PostgreSQL database (saving the order) and publish an `OrderCreated` event to RabbitMQ. If the database save succeeds but the network connection to RabbitMQ drops before publishing, the inventory is never reserved, resulting in a silent failure. Conversely, if the event is published but the database transaction fails to commit, the inventory is reserved for a non-existent order.
* **Solution**: Implemented the **Transactional Outbox Pattern** using MassTransit's outbox integration:
  1. The database write and the creation of the outbox message are done inside a **single SQL transaction**.
  2. The outbox message is stored in an `OutboxMessages` table in the same database.
  3. A background message dispatcher reads from this table, publishes to RabbitMQ, and marks it as dispatched.
  * **Result**: Guarantees **at-least-once message delivery** and eventual consistency, eliminating data loss during network hiccups or service crashes.

### Case Study 2: Concurrency & Inventory Overselling Prevention
* **Problem**: During high-traffic events (e.g., flash sales), multiple users may try to buy the last remaining item simultaneously, leading to race conditions and overselling (negative stock values).
* **Solution**: Implemented a **two-phase inventory reservation** system using PostgreSQL row-level locks:
  1. **Phase 1 (Reserve)**: When an order is placed, `ProductService` intercepts the saga command, locks the product row via an atomic update (`UPDATE Products SET Stock = Stock - ReservedQty WHERE Id = @Id AND Stock >= ReservedQty`), and marks the stock as reserved.
  2. **Phase 2 (Commit or Release)**: If the payment webhook confirms success, the reserved quantity is permanently committed (moved to sold stock). If the payment fails or the session times out, the saga triggers a compensating command to release the reserved stock back to available stock.
  * **Result**: Prevents overselling and race conditions under high concurrent checkout volumes.

### Case Study 3: Cluster-Safe Order Expiration (Quartz.NET Integration)
* **Problem**: Unpaid orders must expire after 5 minutes to release reserved inventory. Using in-memory timers (`System.Timers.Timer`) in a horizontally scaled environment fails because:
  - If the service instance hosting the timer crashes, the timer is lost, leaking reserved stock.
  - Multi-instance scaling causes duplicate timers to trigger.
* **Solution**: Integrated **Quartz.NET** persistent job scheduling with MassTransit:
  - The Saga State Machine schedules an `OrderPaymentExpired` event in Quartz on PostgreSQL when an order is created.
  - Quartz persists this job in SQL tables, making it resilient to service crashes.
  - When the timer fires, Quartz publishes the event. The Saga handles the event, cancels the order, and publishes a compensating event to release stock. If the user pays before expiration, the saga cancels the scheduled job.

### Case Study 4: Low-Latency AI Recommendation Engine (FastAPI + pgvector)
* **Problem**: Generating real-time, semantically-related product recommendations dynamically without straining primary transactional databases.
* **Solution**: Built a hybrid Python + PostgreSQL pipeline:
  1. When products are created or viewed, a C# gRPC client forwards metadata to a Python **VectorService** (FastAPI).
  2. The Python service runs a lightweight transformer model (`all-MiniLM-L6-v2`) to generate 384-dimensional dense vector embeddings.
  3. The embeddings are stored in PostgreSQL using the **pgvector** extension.
  4. Finding similar products is executed directly in PostgreSQL using cosine distance operator (`<=>`):
     ```sql
     SELECT id, name FROM product_vectors 
     ORDER BY embedding <=> @TargetEmbedding 
     LIMIT 5;
     ```
  * **Result**: Semantic-based recommendation search runs in single-digit milliseconds directly inside the database.

---

## Database & Scaling Strategy

- **PostgreSQL (Transactional Data)**: Used by most .NET services. Provides strong ACID guarantees and handles vector math at scale with the `pgvector` extension for `RecommendationService`.
- **MongoDB (Search Index)**: Catalog data is denormalized and synchronized to MongoDB. MongoDB's document-store model and high-speed retrieval index power full-text search in `SearchService` without burdening SQL databases.
- **Redis (Real-Time scale-out)**: Acts as the backplane for **SignalR**. Since clients connect to different instances of the `NotificationService` and `CommentService` behind the gateway, Redis acts as a distributed pub/sub to route live updates (notifications, comments) to the correct client websocket.

---

## Live Demo & Local Quickstart

### Deployment & Live Demo
This microservices backend is deployed to a cloud environment:
* **API Gateway & Swagger Docs**: [https://your-swagger-link.com](https://your-swagger-link.com)
* **Live Client Application**: [https://your-live-demo-link.com](https://your-live-demo-link.com)

### Local Development Quickstart
If you wish to run this project locally, ensure you have the following prerequisites installed:
* **SDKs**: .NET 9 SDK, Python 3.10+
* **Infrastructure Tools**: Docker Desktop, PowerShell 5.1+, Stripe CLI (for payment testing)

#### 1. Setup Infrastructure
Uncomment the services (`postgres`, `mongodb`, `rabbitmq`) in [docker-compose.yml](file:///d:/Projects/TechStore/docker-compose.yml) and start them:
```bash
docker compose up -d
```

#### 2. Run Database Migrations
Use the root helper script to run EF Core database updates across all C# microservices:
```powershell
.\update-all-database.ps1
```

#### 3. Start Backend Services
Start all 11 .NET microservices simultaneously:
```powershell
.\start-all.ps1 -OpenExternalWindow
```

#### 4. Start Python Vector Service
Setup virtual environment, install requirements, and run the FastAPI server:
```powershell
cd server/VectorService
.\start.ps1
```
*(Runs on port 8000 by default, matching `RecommendationService` configuration)*

#### 5. Local Stripe Webhook Forwarding
To test local payments and the Order Saga's response to payment success:
```bash
stripe listen --forward-to http://localhost:5006/webhook/stripe
```

