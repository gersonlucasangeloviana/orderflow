# OrderFlow

Portfólio Full-Stack .NET 8 para gestão de pedidos, construído como monorepo de estudo com Arquitetura Hexagonal.

> Estado atual: fundação e núcleo de domínio implementados. Os adaptadores de banco, Redis, RabbitMQ, MongoDB, gRPC e as UIs estão definidos na arquitetura, mas ainda precisam ser concluídos antes de produção.

## Arquitetura

`Adapters/Infrastructure → Application → Domain`. O domínio não conhece HTTP, EF Core, mensageria ou cache. A API é uma porta de entrada REST; persistência e publicação são portas implementadas pela infraestrutura.

```text
Blazor / MVC / Xamarin legado → API → Application → Domain
                                  └→ SQL, Redis, RabbitMQ, gRPC (adapters)
```

## Executar localmente

1. Instale o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e Docker.
2. Copie `.env.example` para `.env` e troque as senhas locais.
3. Execute `docker compose --env-file .env -f deploy/docker-compose.yml up -d`.
4. Execute `dotnet test` e `dotnet run --project src/OrderFlow.Api`.

A API expõe `/health`, `/swagger` e os endpoints iniciais `/api/products` e `/api/orders`.

## Módulo legado

`legacy/OrderFlow.Mobile.Xamarin` é apenas demonstração: Xamarin.Forms encerrou suporte em maio de 2024. Ele não entra no deploy do Dokploy e deverá migrar para .NET MAUI conforme [o plano](docs/xamarin-to-maui.md).

## Documentação

- [Arquitetura](docs/architecture.md)
- [Guia de aprendizado](docs/learning-guide.md)
- [Estratégia de testes](docs/testing-strategy.md)
- [Progresso por fase](docs/progress.md)
- [Decisões WASM](docs/wasm-performance.md)

## Deploy Dokploy

No Dokploy, publique somente API e frontends atrás do proxy. Mantenha SQL Server, Redis, RabbitMQ e MongoDB exclusivos da rede interna; defina os segredos como variáveis do projeto e faça backup regular dos volumes SQL/Mongo antes de upgrades.
