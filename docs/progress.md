# Progresso

| Fase | Resultado | Validação |
|---|---|---|
| 0 Fundação | Estrutura, solution, Compose, documentação e CI inicial | Pendente de SDK local |
| 1 Domínio/TDD | Regras de pedido e quatro testes de domínio | Pendente de SDK local |
| 2 API/SQL | API mínima e portas; EF/Identity/migrations pendentes | Não iniciada integralmente |
| 3–8 | Frontends, gRPC, RabbitMQ, logs, .NET MAUI e hardening | MAUI Android iniciado; validação móvel depende da workload `maui-android` |

Validação local: API em Release e 4 testes de domínio passaram. O SDK instalado pelo Homebrew não expõe manifestos de workloads MAUI, portanto a compilação Android deve ser executada em uma instalação oficial do SDK com `dotnet workload install maui-android`.
