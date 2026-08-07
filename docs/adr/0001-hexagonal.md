# ADR 0001 — Arquitetura Hexagonal

**Decisão:** casos de uso dependem de portas e o domínio não referencia infraestrutura. **Consequência:** testes de regra são rápidos e os adaptadores podem mudar; há mais interfaces do que numa aplicação CRUD simples.
