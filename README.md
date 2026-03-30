# Cerberus SSO - A SSO Authentication System

A modern authentication platform built using **Clean Architecture** and **CQRS**, designed to support scalable **Single Sign-On (SSO)** and **Multi-Factor Authentication (MFA)** flows. The system focuses on clear separation of concerns, extensibility, and production-grade design patterns.

[DEMO VIDEO]

This project implements a centralized identity service responsible for authentication, authorization, and secure user management across multiple clients.

Built with a strong architectural foundation, it emphasizes:

- Decoupled layers and dependency direction
- Scalable authentication flows
- Extensibility for real-world identity scenarios (MFA, external providers, distributed systems)

## Technologies

- **.NET 10 / ASP.NET Core**
- **Clean Architecture**
- **CQRS Pattern**
- **PostgreSQL (Write DB)**
- **MongoDB (Read DB)**
- **Redis (caching / session support)**
- **Apache Kafka (Producer & Consumer)**
- **Kibana / ELK Stack (centralized logging)**

## Architecture Highlights

- **Layered Clean Architecture**
  Strict separation between Domain, Application, Infrastructure, and API layers

- **CQRS with Database Segregation**
  Optimized read and write paths using dedicated data stores

- **Event-Driven Design**
  Kafka-based messaging with binary serialization for efficient communication

- **Resilient Backend Design**
  Outbox pattern combined with message-driven processing

- **Concurrency Control & Idempotency**
  Use of concurrency tokens / indexes to ensure idempotent operations and prevent duplicate event processing

## Custom Implementations

- **Manual CQRS Handler Registration**
  Explicit and lightweight handler resolution without heavy abstractions

- **Outbox Pattern**
  Ensures reliable event handling and consistency between state changes and message dispatching

- **Event Projector System**
  Dynamically maps domain events to read model updates using scoped resolution

- **Kafka Integration (Binary Serialization)**
  High-performance producer/consumer pipeline for inter-service communication

- **Authentication & Security Layer**
  - BCrypt password hashing through BouncyCastle library
  - SHA-256 hashing utilities
  - JWT token generation and validation

- **MFA Implementation**
  QR code generation for authenticator app Google Authenticator

- **Cross-Cutting Concerns**
  - Global exception handling middleware
  - Rate limiting for API protection
  - Structured logging with Kibana integration

## Authentication Flow

1. Client redirects user to authentication service
2. User credentials are validated through the API
3. Optional MFA step is triggered (QR-based setup & validation)
4. JWT token is issued upon successful authentication
5. Client applications validate and consume the token

---
