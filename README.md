# MegaSena Hub

Projeto de estudo para praticar desenvolvimento orientado a IA, Clean Architecture, testes automatizados, Web API, workers, mensageria e análise histórica da Mega-Sena.

> **Aviso:** Este projeto não tem objetivo de prever resultados da Mega-Sena nem de aumentar chances reais de ganho. O foco é análise histórica, geração de combinações e controle de apostas como domínio de estudo.

---

## Objetivo do sistema

No estado final, o sistema irá:

- Sincronizar resultados históricos da Mega-Sena a partir de fontes externas;
- Identificar e preencher concursos faltantes na base de dados;
- Publicar mensagens no RabbitMQ para processamento assíncrono;
- Consumir mensagens e gravar resultados na base de dados;
- Gerar combinações de jogos com diferentes estratégias (aleatório, mais sorteados, menos sorteados, etc.);
- Verificar se uma combinação já saiu em algum concurso histórico;
- Verificar se apostas cadastradas teriam sido premiadas em concursos passados;
- Permitir cadastro de usuário e controle de apostas com autenticação.

---

## Arquitetura planejada

O projeto segue **Clean Architecture / Onion Architecture**, onde as dependências sempre apontam para o centro (Domain).

```
┌─────────────────────────────────────────────────────────────┐
│                          API / Workers                       │
│  ┌───────────────────────────────────────────────────────┐  │
│  │                      Application                      │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │                 Infrastructure                  │  │  │
│  │  │  ┌───────────────────────────────────────────┐  │  │  │
│  │  │  │               Domain  ← núcleo            │  │  │  │
│  │  │  └───────────────────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

| Camada           | Responsabilidade                                                   |
|------------------|--------------------------------------------------------------------|
| `Domain`         | Entidades, value objects, enums e regras de negócio puras          |
| `Application`    | Casos de uso, DTOs, interfaces de repositório e serviços           |
| `Infrastructure` | EF Core, repositórios, clientes HTTP, adaptadores de mensageria    |
| `API`            | Controllers, middleware, configuração da Web API                   |
| `Worker.Sync`    | Worker de sincronização periódica de resultados                    |
| `Worker.Consumer`| Consumer de mensagens do RabbitMQ                                  |
| `Contracts`      | Mensagens e eventos compartilhados entre camadas e workers         |

---

## Etapa atual — Domínio

Nesta primeira etapa foram implementados:

- Entidades de domínio;
- Value objects;
- Enums;
- Regras de negócio;
- Testes unitários do domínio.

### Estrutura criada

```
backend/
├── src/
│   └── MegaSenaHub.Domain/
│       ├── Entities/
│       │   ├── LotteryContest.cs
│       │   ├── LotteryContestNumber.cs
│       │   ├── PrizeRange.cs
│       │   ├── UserBet.cs
│       │   └── UserBetResult.cs
│       ├── Enums/
│       │   ├── GameGenerationStrategy.cs
│       │   ├── LotteryType.cs
│       │   ├── PrizeRangeType.cs
│       │   └── UserBetStatus.cs
│       ├── Exceptions/
│       │   └── DomainException.cs
│       └── ValueObjects/
│           ├── CombinationHash.cs
│           ├── MegaSenaNumber.cs
│           └── MegaSenaNumbers.cs
└── tests/
    └── MegaSenaHub.Domain.Tests/
        ├── Entities/
        │   ├── LotteryContestTests.cs
        │   ├── PrizeRangeTests.cs
        │   ├── UserBetResultTests.cs
        │   └── UserBetTests.cs
        └── ValueObjects/
            ├── CombinationHashTests.cs
            ├── MegaSenaNumbersTests.cs
            └── MegaSenaNumberTests.cs
```

---

## Regras de domínio implementadas

- Números da Mega-Sena devem estar entre **1 e 60**;
- Concursos e apostas do MVP devem ter **exatamente 6 números**;
- Não pode haver **números repetidos** em uma combinação;
- Combinações são normalizadas por um **hash ordenado** no formato `01-02-03-04-05-06`;
- Faixas de premiação válidas: **Quadra (4 acertos)**, **Quina (5 acertos)**, **Sena (6 acertos)**;
- O status inicial de uma aposta é **Pending**;
- Ao verificar uma aposta, o status é atualizado para `WonSena`, `WonQuina`, `WonQuadra` ou `Lost`;
- `TotalPrize` e `PrizeAmount` não podem ser negativos;
- Todas as violações de regra lançam `DomainException`.

---

## Como rodar os testes

```bash
dotnet test
```

Para ver saída detalhada:

```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## Próximas etapas

- [ ] Application — casos de uso (sincronizar concursos, verificar aposta, gerar jogo);
- [ ] EF Core — mapeamentos e migrations via Infrastructure;
- [ ] Web API — endpoints REST;
- [ ] RabbitMQ — publicação e consumo de mensagens;
- [ ] Workers — `Worker.Sync` e `Worker.Consumer`;
- [ ] Autenticação com ASP.NET Identity + JWT;
- [ ] Docker Compose para orquestração local.
