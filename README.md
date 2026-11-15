# Mini EFT Transfer API

A small money-transfer API between accounts, built with **ASP.NET Core 8** and **PostgreSQL**. Amounts are whole Iraqi dinars (IQD, no decimals).

## Run

Requires Docker.

```bash
docker compose up --build -d
```

Open Swagger at **http://localhost:8080/swagger**. On startup the database is created and 4 demo accounts are added, each with 1,000,000 IQD. Their ids are listed at the top of the Swagger page.

To stop it: `docker compose down`. Add `-v` to also delete the data.

## Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/transfers` | Transfer money between two accounts. Requires the `Idempotency-Key` header. |
| `GET` | `/transfers/{transferId}` | Get a transfer and its status: `Pending`, `Completed`, `Failed` or `Reversed`. |
| `POST` | `/transfers/{transferId}/reverse` | Reverse a `Completed` transfer. |
| `GET` | `/accounts/{accountId}/statement` | Balance and movements, newest first. Optional `from`, `to`, `page` and `pageSize`. |

**Rules**

- The minimum transfer is **1,000 IQD**.
- **Not enough money:** the transfer is saved as `Failed` (reason `INSUFFICIENT_FUNDS`) and returned with **201**. No money moves.
- **Same `Idempotency-Key`:**
  - with the same body, the original transfer is returned with **200** and the money is not moved twice;
  - with a different body, the response is **409**.
- **Reverse:** only `Completed` transfers can be reversed, and the receiver must still have enough balance. Otherwise the response is **409**.
- **Errors** are returned as `ProblemDetails`: 400 for invalid input, 404 when not found, 409 for conflicts.

## Design

Clean Architecture with four projects:

| Project | Contents |
|---|---|
| `MiniEft.Domain` | Entities and business rules (`Account`, `Transfer`, `LedgerEntry`, `Money`). |
| `MiniEft.Application` | Use cases (`TransferService`, `StatementService`) and the interfaces they depend on. |
| `MiniEft.Infrastructure` | EF Core with PostgreSQL, the repositories, and migrations. |
| `MiniEft.Api` | Controllers, error handling and Swagger. |

- Every transfer writes **double-entry ledger** rows: a debit and a credit, each with `balance_after`.
- Concurrent transfers are made safe by **row locks** (`FOR NO KEY UPDATE`), always taken in the same order to avoid deadlocks.

## Run without Docker for the API

```bash
docker compose up -d db
dotnet run --project src/MiniEft.Api
```
