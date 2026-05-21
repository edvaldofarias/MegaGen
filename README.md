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

## Etapa 1 — Domain

Nesta etapa foram implementados:

- Entidades de domínio: `LotteryContest`, `LotteryContestNumber`, `PrizeRange`, `UserBet`, `UserBetResult`;
- Value objects: `MegaSenaNumber`, `MegaSenaNumbers`, `CombinationHash`;
- Enums: `LotteryType`, `UserBetStatus`, `GameGenerationStrategy`, `PrizeRangeType`;
- Exceção base: `DomainException`;
- **76 testes unitários** com xUnit + FluentAssertions.

### Estrutura — Etapa 1

```
backend/
├── src/
│   └── MegaSenaHub.Domain/
│       ├── Entities/           (LotteryContest, LotteryContestNumber, PrizeRange, UserBet, UserBetResult)
│       ├── Enums/              (GameGenerationStrategy, LotteryType, PrizeRangeType, UserBetStatus)
│       ├── Exceptions/         (DomainException)
│       └── ValueObjects/       (CombinationHash, MegaSenaNumber, MegaSenaNumbers)
└── tests/
    └── MegaSenaHub.Domain.Tests/
        ├── Entities/           (LotteryContestTests, PrizeRangeTests, UserBetResultTests, UserBetTests)
        └── ValueObjects/       (CombinationHashTests, MegaSenaNumbersTests, MegaSenaNumberTests)
```

---

## Etapa 2 — Application use cases

Nesta etapa foram implementados a camada de Application com interfaces, DTOs, commands, queries, mensagens e 7 casos de uso, além de testes com mocks usando **NSubstitute**.

### Modificações no Domain (retrocompatíveis)

- `UserBet`: adicionada propriedade `PrizeWon { get; }` para registrar o valor ganho;
- `UserBet.CheckAgainstContest`: novo overload com parâmetros `DateTimeOffset checkedAt` e `decimal prizeWon = 0m`, permitindo uso de `IClock` nos use cases. O overload original sem parâmetros foi mantido para compatibilidade.

### Interfaces de abstração (`Abstractions/`)

| Interface             | Responsabilidade                                                |
|-----------------------|-----------------------------------------------------------------|
| `IClock`              | Abstrai `DateTimeOffset.UtcNow` para testes determinísticos     |
| `ICurrentUser`        | Expõe `UserId` e `IsAuthenticated` do usuário autenticado      |
| `IMessagePublisher`   | Publica mensagens no broker de mensageria                       |
| `ILotteryResultProvider` | Fonte externa de resultados (API da Caixa, etc.)           |
| `IContestRepository`  | Persistência de `LotteryContest`                                |
| `IUserBetRepository`  | Persistência de `UserBet`                                       |

### DTOs (`DTOs/`)

`LotteryContestResultDto`, `PrizeRangeDto`, `NumberFrequencyDto`, `GeneratedGameDto`, `ImportContestResultDto`, `SyncMissingContestsResultDto`, `BetHistoryCheckResultDto`, `UserBetSummaryDto`, `UserBetDto`.

### Commands e Queries

| Tipo    | Nome                          |
|---------|-------------------------------|
| Command | `GenerateMegaSenaGamesCommand` |
| Command | `RegisterUserBetCommand`      |
| Command | `CheckUserBetResultCommand`   |
| Command | `ImportContestResultCommand`  |
| Command | `SyncMissingContestsCommand`  |
| Query   | `CheckCombinationHistoryQuery`|
| Query   | `GetUserBetSummaryQuery`      |

### Mensagem (`Messaging/`)

`ContestSyncRequestedMessage` — publicada para cada concurso faltante durante a sincronização.

### Casos de uso (`UseCases/`)

| Use Case                          | Descrição                                                                 |
|-----------------------------------|---------------------------------------------------------------------------|
| `SyncMissingContestsUseCase`      | Detecta concursos ausentes e publica mensagem por número faltante         |
| `ImportContestResultUseCase`      | Importa um concurso do provider externo e persiste na base                |
| `GenerateMegaSenaGamesUseCase`    | Gera jogos com 5 estratégias + flags de evitação de histórico             |
| `RegisterUserBetUseCase`          | Registra aposta do usuário autenticado                                    |
| `CheckUserBetResultUseCase`       | Verifica resultado da aposta com `IClock` para `CheckedAt` correto        |
| `CheckCombinationHistoryUseCase`  | Consulta histórico de acertos para uma combinação                         |
| `GetUserBetSummaryUseCase`        | Retorna resumo consolidado de apostas: gasto, ganho, saldo, melhor resultado |

### Estratégias de geração de jogos

| Estratégia  | Lógica                                                                  |
|-------------|-------------------------------------------------------------------------|
| `Random`    | 6 números aleatórios distintos entre 1-60                               |
| `MostDrawn` | Prioriza os números mais sorteados historicamente                       |
| `LeastDrawn`| Prioriza os números menos sorteados historicamente                      |
| `Mixed`     | Metade dos mais sorteados + metade aleatória do restante                |
| `NeverDrawn`| Aleatório, rejeitando combinações que já saíram                         |
| `NeverWon`  | Aleatório, rejeitando combinações que teriam 4+ acertos em qualquer sorteio |

Flags adicionais: `AvoidAlreadyDrawnCombination` e `AvoidAlreadyWonCombination` aplicam os mesmos filtros sobre qualquer estratégia.

### Estrutura — Etapa 2

```
backend/
├── src/
│   └── MegaSenaHub.Application/
│       ├── Abstractions/   (IClock, ICurrentUser, IContestRepository, IUserBetRepository,
│       │                    ILotteryResultProvider, IMessagePublisher)
│       ├── Commands/       (5 commands)
│       ├── DTOs/           (9 records)
│       ├── Exceptions/     (AppException, NotFoundException, UnauthorizedException)
│       ├── Messaging/      (ContestSyncRequestedMessage)
│       ├── Queries/        (2 queries)
│       └── UseCases/       (7 use cases)
└── tests/
    └── MegaSenaHub.Application.Tests/
        └── UseCases/       (7 test classes, 50 testes)
```

**50 testes de Application** com NSubstitute (mocks) e FluentAssertions.

---
- Enums;
- Regras de negócio;
- Testes unitários do domínio.

### Estrutura criada — Etapa 1

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
- `TotalPrize`, `PrizeAmount` e `PrizeWon` não podem ser negativos;
- Todas as violações de regra lançam `DomainException`.

---

## Como rodar os testes

```bash
dotnet test
```

Para saída detalhada:

```bash
dotnet test --logger "console;verbosity=normal"
```

**Resultado atual:** `126 testes — 76 Domain + 50 Application — todos passando`.

---

## Próximas etapas

- [x] ~~Etapa 1: Domain — entidades, value objects, enums, regras e 76 testes~~
- [x] ~~Etapa 2: Application — 7 use cases, 6 interfaces, 9 DTOs, 50 testes com mocks~~
- [ ] Etapa 3: Infrastructure — EF Core, repositórios, migrations, clientes HTTP;
- [ ] Etapa 4: API — endpoints REST, autenticação JWT, middleware;
- [ ] Etapa 5: RabbitMQ — `IMessagePublisher`, consumers, dead-letter;
- [ ] Etapa 6: Workers — `Worker.Sync` e `Worker.Consumer`;
- [ ] Docker Compose para orquestração local.
