# Estratégia de testes

Unitários validam regras de domínio rapidamente, sem infraestrutura. Integração usa Testcontainers (SQL Server, Redis, RabbitMQ e MongoDB) para validar adaptadores e transações. Contratos asseguram REST e gRPC entre serviços. E2E valida os fluxos críticos pelo frontend. O ciclo é Red → Green → Refactor: cada regra de domínio começa por um teste que falha.
