# Guia de aprendizado

- **Encapsulamento:** `Order` controla transições e itens; consumidores não alteram seu estado diretamente.
- **SRP:** `CreateOrder` orquestra um caso de uso; entidade aplica regras.
- **DIP / Ports and Adapters:** repositório, outbox e frete são interfaces na Application; EF/RabbitMQ/gRPC ficam fora do núcleo.
- **Repository:** simplifica persistência transacional; uma alternativa simples seria um DbContext diretamente no caso de uso, inadequada por acoplar EF.
- **Outbox:** evita perder a notificação após commit; uma publicação RabbitMQ direta é menor, porém inconsistente sob falha.
- **Strategy:** fretes e notificadores variam sem mudar o caso de uso; não criar estratégias para uma única regra estável.
- **Decorator:** cache e logging devem envolver as portas sem alterar sua implementação.
