# Arquitetura

O núcleo contém regras de pedido e não possui dependências de infraestrutura. A camada Application expõe casos de uso e portas (`IOrderRepository`, `INotificationOutbox`, `IFreightQuoteProvider`). Infrastructure implementará adaptadores EF Core, Redis, RabbitMQ e gRPC.

Outbox salva `NotificationRequested` na mesma transação do pedido. Um publicador com retry encaminha para `notifications.requested`; o consumidor idempotente usa `MessageId`, ACK/NACK e DLQ. Logs técnicos seguem `ILogger → buffer limitado → logs.events → worker → MongoDB`, com prioridade para erros. Auditoria de confirmação/cancelamento permanece no SQL/Outbox.

Autenticação inicial será ASP.NET Core Identity + JWT, roles `Admin` e `Customer`; a evolução planejada é Keycloak via OIDC/OAuth 2. Cada middleware deve aceitar `X-Correlation-Id` ou gerar um UUID.
