# Progresso

| Fase | Resultado | Validação |
|---|---|---|
| 0 Fundação | Monorepo, solution, Compose, CI, ADR e PlantUML | Estrutura verificada |
| 1 Domínio/TDD | Regras de pedido e quatro testes de domínio | 4 testes aprovados |
| 2 API/SQL | EF Core SQL Server, migrations, esquema transacional, catálogo, Outbox, Identity e JWT | Build da API aprovado; endpoints de auth e policy Admin implementados |
| 3 Frontends | Blazor WASM com catálogo REST e paginação, MVC e MAUI Android estruturados | Web e MVC compilam; carrinho/login e MAUI dependem das próximas etapas/workload |
| 4 Frete | Contrato Protobuf compartilhado, serviço gRPC e adapter cliente com timeout/503 | Builds API e Freight aprovados |
| 5 Notificações | Publicador da Outbox, RabbitMQ, fila durável, ACK/NACK, retry limitado, DLQ, e-mail simulado e idempotência SQL por `MessageId` | Builds API/Worker e Compose aprovados |
| 6 Logs | Serilog, buffer limitado, publicação RabbitMQ, Worker Mongo em lote, índices e TTL de 30 dias | Builds API/Worker aprovados; retry durável do lote Mongo ainda pendente |
| 7 MAUI | Cliente Android e HTTP isolado | Workload Android pendente localmente |
| 8 Publicação | Dockerfiles, Compose produção e CI | Compose e imagens devem ser exercitados com Docker |

Validação local: API, Web, MVC, Freight e Workers compilam em Release; 4 testes de domínio passaram. O SDK instalado pelo Homebrew não expõe manifestos de workloads MAUI, portanto a compilação Android deve ser executada em uma instalação oficial do SDK com `dotnet workload install maui-android`.
