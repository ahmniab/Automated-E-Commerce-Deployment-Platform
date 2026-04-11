# eShop.IntegrationEventLogEF

`eShop.IntegrationEventLogEF` is a reusable .NET library for persisting and tracking integration events with Entity Framework Core.

It provides:

- Integration event log entity and state management.
- Event log service abstraction and implementation.
- Transaction support helpers for resilient event publishing flows.

## Target Framework

- `net10.0`

## Package Description

Library for persisting and tracking integration events using Entity Framework Core in eShop-based microservices.

## Notes

This package depends on:

- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `eShop.EventBus`
