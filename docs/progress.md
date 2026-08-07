# Progresso

| Fase | Resultado | Validação |
|---|---|---|
| 0 Fundação | Monorepo, solution, Compose, CI, ADR e PlantUML | Estrutura verificada |
| 1 Domínio/TDD | Regras de pedido e quatro testes de domínio | 4 testes aprovados |
| 2 API/SQL | API, portas e projeto Infrastructure | Build da API aprovado; EF/Identity/migrations pendentes |
| 3 Frontends | Blazor WASM, MVC e MAUI Android estruturados | Web e MVC compilam; MAUI depende da workload |
| 4 Frete | Projeto gRPC e contrato Protobuf com cálculo inicial | Build aprovado |
| 5 Notificações | Worker e contrato presentes | Build aprovado; RabbitMQ/Outbox/DLQ pendentes |
| 6 Logs | Worker de ingestão presente | Build aprovado; Serilog/Mongo pipeline pendente |
| 7 MAUI | Cliente Android e HTTP isolado | Workload Android pendente localmente |
| 8 Publicação | Dockerfiles, Compose produção e CI | Compose e imagens devem ser exercitados com Docker |

Validação local: API, Web, MVC, Freight e Workers compilam em Release; 4 testes de domínio passaram. O SDK instalado pelo Homebrew não expõe manifestos de workloads MAUI, portanto a compilação Android deve ser executada em uma instalação oficial do SDK com `dotnet workload install maui-android`.
