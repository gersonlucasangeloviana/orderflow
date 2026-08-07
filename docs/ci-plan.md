# Plano de CI no GitHub Actions

## Objetivo

Impedir que código quebrado, imagens que não constroem e segredos versionados cheguem à `main`. O workflow em `.github/workflows/ci.yml` roda em pull requests e após push na `main`.

## Gates implementados

| Job | Runner | Garante |
|---|---|---|
| `server` | Ubuntu | Restore, build Release e testes dos módulos de servidor/Web. |
| `docker` | Ubuntu | Todas as seis imagens implantáveis constroem e o Compose é válido. |
| `mobile` | macOS | O app MAUI Android compila com a workload correta. |
| `secrets` | Ubuntu | Gitleaks bloqueia chaves e credenciais versionadas. |

O MAUI é mantido em runner macOS porque a cadeia de ferramentas móvel não deve bloquear a validação dos serviços de servidor no Linux.

## Configuração necessária no GitHub

1. Em **Settings → Actions → General**, habilite GitHub Actions e permita ações verificadas.
2. Em **Settings → Branches**, crie uma regra para `main` exigindo pull request e os checks `Build and test server modules`, `Verify Docker images`, `Build MAUI Android` e `Scan secrets`.
3. Exija branch atualizada antes do merge e limite bypass a administradores somente se necessário.
4. Não cadastre `SQL_SA_PASSWORD`, `Jwt__Key`, URI RabbitMQ ou MongoDB como secrets de CI: o pipeline não implanta nem acessa infraestrutura real. Esses valores pertencem apenas ao Dokploy/ambiente de runtime.
5. Para publicar imagens no futuro, adicione `GHCR_TOKEN` e permissões `packages: write` somente ao job de release na `main`; CI de PR nunca deve publicar.

## Próximas evoluções recomendadas

- Habilitar Dependabot para NuGet e GitHub Actions.
- Adicionar CodeQL para C#.
- Trocar os testes de integração marcados como `Skip` por Testcontainers e executá-los no job `server`.
- Exigir relatório de cobertura quando a suíte de aplicação crescer.
- Criar um workflow de `deploy.yml` separado, acionado somente após merge na `main`, com secrets exclusivos do Dokploy.

## Execução local equivalente

```bash
dotnet build src/OrderFlow.Api -c Release
dotnet test tests/OrderFlow.Domain.Tests -c Release
docker compose --env-file .env.example -f deploy/docker-compose.yml -f deploy/docker-compose.production.yml config --quiet
```
