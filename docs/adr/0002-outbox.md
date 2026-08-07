# ADR 0002 — Outbox para notificações

**Decisão:** persistir a intenção de publicar com o pedido. **Consequência:** evita inconsistência entre SQL e RabbitMQ, ao custo de um processo publicador e idempotência do consumidor.
