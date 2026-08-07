# Dokploy

Crie um Compose Application a partir de `deploy/docker-compose.yml` e aplique também `docker-compose.production.yml`. Exponha somente API, Web e MVC através do proxy. SQL Server, Redis, RabbitMQ e MongoDB devem ficar na rede interna, com segredos configurados no painel — nunca no Git.
