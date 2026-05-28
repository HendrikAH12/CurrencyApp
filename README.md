# CurrencyApp

REST API for managing users, currency holdings, and portfolio totals converted to a main currency.

Built with **.NET 10**, **ASP.NET Core**, **Entity Framework Core**, and **SQLite**. Exchange rates are fetched from the [Frankfurter API](https://www.frankfurter.app/) and cached in the database for 30 minutes.

## Features

- CRUD for currencies and users
- User holdings (amount per currency)
- Main currency per user
- Portfolio total in main currency on `GET /users/{id}` (calculated at request time)
- Exchange rate cache per currency pair (`From` → `To`) with 30-minute TTL
- Graceful conversion failures: if a rate cannot be resolved, `totalInMainCurrency` is `null` and the user is still returned

## Architecture

Clean / layered structure:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `CurrencyApp.Domain` | Entities, domain rules, repository contracts |
| **Application** | `CurrencyApp.Application` | Use cases, DTOs, `Result` pattern, service contracts |
| **Infrastructure** | `CurrencyApp.Infra` | EF Core, SQLite, repositories, Frankfurter HTTP client |
| **API** | `CurrencyApp.Api` | Controllers, middleware, DI composition |

**Dependencies:** `Api` → `Application` + `Infra` → `Application` + `Domain` → (none)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (for migrations)

```bash
dotnet tool install --global dotnet-ef
```

## Getting started

### 1. Restore and build

```bash
dotnet restore
dotnet build
```

### 2. Apply database migrations

From the repository root:

```bash
dotnet ef database update --project src/CurrencyApp.Infra --startup-project src/CurrencyApp.Api
```

SQLite file: `currencyapp.db` (created in the API working directory when you run the app).

### 3. Run the API

```bash
dotnet run --project src/CurrencyApp.Api
```

- HTTP: `http://localhost:5124`
- HTTPS: `https://localhost:7225`
- Swagger UI: `/swagger`

### 4. Run tests

```bash
dotnet test
```

## API overview

Base URL: `http://localhost:5124` (default)

### Currencies

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/currencies` | List currencies (cursor pagination) |
| `GET` | `/currencies/{id}` | Get currency by id |
| `POST` | `/currencies` | Create currency |
| `DELETE` | `/currencies/{id}` | Delete currency |

### Users

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/users` | List users (cursor pagination) |
| `GET` | `/users/{id}` | Get user with holdings and `totalInMainCurrency` |
| `POST` | `/users` | Create user |
| `PUT` | `/users/{id}` | Update user (name, main currency) |
| `DELETE` | `/users/{id}` | Delete user |
| `POST` | `/users/{id}/currencies/{currencyId}` | Add or update holding amount |
| `DELETE` | `/users/{id}/currencies/{currencyId}` | Remove holding |

### Response envelope

Success:

```json
{ "data": { } }
```

Error:

```json
{ "error": "message" }
```

Paginated lists include `nextCursor` when applicable.

## Exchange rates

Flow for `GET /users/{id}` when a main currency is set:

1. For each holding, resolve rate `holding → main` via `IExchangeRateService`
2. Use cached rate if valid (`ExpiresAtUtc` not passed)
3. Otherwise call Frankfurter, store/update cache, commit
4. Sum `amount × rate` and set `totalInMainCurrency`
5. If any rate resolves to `0` (provider/cache failure), `totalInMainCurrency` is `null`

Same currency pair (`USD` → `USD`) uses rate `1` without an external call.
