# MegaSena Hub — Backend

API REST para análise histórica, geração de jogos e controle de apostas da **Mega-Sena**.

## Arquitetura

Clean Architecture em 4 camadas:

```
Domain  →  Application  →  Infrastructure  →  API
```

| Projeto | Responsabilidade |
|---|---|
| `MegaSenaHub.Domain` | Entidades, value objects, regras de negócio puras |
| `MegaSenaHub.Application` | Use cases, DTOs, interfaces de repositório |
| `MegaSenaHub.Infrastructure` | EF Core, PostgreSQL, Identity, JWT, adapters NoOp |
| `MegaSenaHub.Api` | Controllers ASP.NET Core, autenticação/autorização, Swagger |

## Stack

- **.NET 9 / C# 13** — file-scoped namespaces, records, primary constructors
- **ASP.NET Core 9 Web API** — Controllers
- **ASP.NET Core Identity** — gerenciamento de usuários com PostgreSQL
- **JWT Bearer** — autenticação stateless
- **EF Core 9 + Npgsql** — ORM com convenção snake_case
- **Swagger / OpenAPI** — documentação interativa em `/swagger`
- **xUnit 2.9 + FluentAssertions + NSubstitute** — testes unitários
- **WebApplicationFactory + Testcontainers** — testes de integração da API

## Endpoints

### Autenticação (`/api/auth`)
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/register` | Cria conta e retorna token JWT |
| `POST` | `/api/auth/login` | Autentica e retorna token JWT |

### Concursos (`/api/mega-sena/contests`)
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `GET` | `/api/mega-sena/contests/latest` | Não | Último concurso registrado |
| `GET` | `/api/mega-sena/contests/{number}` | Não | Concurso por número |
| `POST` | `/api/mega-sena/contests/sync` | Não | Sincroniza concursos faltantes |

### Jogos (`/api/mega-sena/games`)
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `POST` | `/api/mega-sena/games/generate` | Não | Gera jogos por estratégia |

### Combinações (`/api/mega-sena/combinations`)
| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| `POST` | `/api/mega-sena/combinations/check` | Não | Verifica histórico de combinação |

### Apostas do Usuário (`/api/me/bets`) — requer JWT
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/me/bets` | Registra nova aposta |
| `GET` | `/api/me/bets` | Lista apostas do usuário |
| `GET` | `/api/me/bets/{id}` | Aposta por ID |
| `POST` | `/api/me/bets/{id}/check` | Verifica resultado da aposta |

### Perfil (`/api/me`) — requer JWT
| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/me/balance` | Resumo financeiro das apostas |

## Como executar

### Pré-requisitos
- .NET 9 SDK
- PostgreSQL 16+

### Banco de dados

```bash
# Criar banco (ou usar docker-compose)
docker run -d \
  --name megasena-pg \
  -e POSTGRES_DB=megasena_hub \
  -e POSTGRES_USER=megasena \
  -e POSTGRES_PASSWORD=megasena123 \
  -p 5432:5432 \
  postgres:16
```

### Migrations

```bash
cd backend
dotnet ef database update \
  --project src/MegaSenaHub.Infrastructure \
  --startup-project src/MegaSenaHub.Api
```

### Executar a API

```bash
cd backend/src/MegaSenaHub.Api
dotnet run
```

Swagger disponível em: `https://localhost:{porta}/swagger`

## Testes

```bash
# Testes unitários (Domain + Application)
cd backend
dotnet test --filter "FullyQualifiedName!~Infrastructure.Tests&FullyQualifiedName!~Api.Tests"

# Todos os testes (requer Docker para Testcontainers)
dotnet test
```

### Cobertura atual
| Projeto | Testes |
|---|---|
| Domain | 76 |
| Application | 50 |
| **Total unitários** | **126** |

## Configuração

### `appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=megasena_hub;Username=megasena;Password=megasena123"
  },
  "Jwt": {
    "Issuer": "MegaSenaHub",
    "Audience": "MegaSenaHub",
    "Secret": "development-only-secret-key-change-this-value-32chars",
    "ExpirationMinutes": 60
  }
}
```

> **Atenção:** nunca versione secrets reais. Use variáveis de ambiente ou Azure Key Vault em produção.

## Estrutura de pastas

```
backend/
├── MegaGen.slnx
├── README.md
├── src/
│   ├── MegaSenaHub.Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   ├── MegaSenaHub.Application/
│   │   ├── Abstractions/
│   │   ├── Commands/
│   │   ├── DTOs/
│   │   ├── Queries/
│   │   └── UseCases/
│   ├── MegaSenaHub.Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   └── MegaSenaHub.Infrastructure/
│       ├── Adapters/
│       ├── Data/
│       ├── Identity/
│       ├── Migrations/
│       └── Repositories/
└── tests/
    ├── MegaSenaHub.Api.Tests/
    ├── MegaSenaHub.Application.Tests/
    ├── MegaSenaHub.Domain.Tests/
    └── MegaSenaHub.Infrastructure.Tests/
```
