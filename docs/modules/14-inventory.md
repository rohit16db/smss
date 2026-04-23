# Module: Inventory Management

## Overview
Track school inventory: categories, items with SKU/quantity/price, and stock movement transactions (in/out). Supports pagination and search.

---

## Domain Entities

### InventoryCategory (`SMS.Domain.Entities.InventoryCategory` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Uniforms", "Books" |

### InventoryItem (`SMS.Domain.Entities.InventoryItem` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Summer Uniform" |
| SKU | string | Stock keeping unit code |
| Description | string? | |
| CategoryId | Guid | FK |
| TotalQuantity | int | Current stock |
| ReorderLevel | int | Default 5 (alert threshold) |
| UnitPrice | decimal | |
| IsActive | bool | |
| *Nav* | Category |

### InventoryTransaction (`SMS.Domain.Entities.InventoryTransaction` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| ItemId | Guid | FK |
| Type | string | "In" or "Out" |
| Quantity | int | |
| Notes | string? | |
| TransactionDate | DateTime | |

---

## API Endpoints

**Controller**: `InventoryController` — Route: `api/inventory`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/inventory/summary` | Inventory dashboard summary |
| GET | `/api/inventory/items` | List items (paginated, searchable) |
| GET | `/api/inventory/categories` | List categories |
| GET | `/api/inventory/transactions` | List transactions (paginated) |
| POST | `/api/inventory/items` | Create item |
| POST | `/api/inventory/categories` | Create category |
| POST | `/api/inventory/transactions` | Record transaction |
| PUT | `/api/inventory/categories/{id}` | Update category |

---

## CQRS (in `Features/Inventory`)

### Commands
- `CreateInventoryItemCommand`, `CreateInventoryCategoryCommand`, `UpdateInventoryCategoryCommand`
- `CreateInventoryTransactionCommand` — ItemId, Type (In/Out), Quantity, Notes

### Queries
- `GetInventorySummaryQuery` — Dashboard stats
- `GetInventoryItemsQuery` — Paginated with search
- `GetInventoryCategoriesQuery`
- `GetInventoryTransactionsQuery` — Paginated

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/InventoryCategory.cs` |
| Entity | `backend/src/SMS.Domain/Entities/InventoryItem.cs` |
| Entity | `backend/src/SMS.Domain/Entities/InventoryTransaction.cs` |
| Commands | `backend/src/SMS.Application/Features/Inventory/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Inventory/DTOs/InventoryDtos.cs` |
| Queries | `backend/src/SMS.Application/Features/Inventory/Queries/GetInventorySummary.cs` |
| Queries | `backend/src/SMS.Application/Features/Inventory/Queries/GetInventoryItems.cs` |
| Queries | `backend/src/SMS.Application/Features/Inventory/Queries/GetInventoryCategories.cs` |
| Queries | `backend/src/SMS.Application/Features/Inventory/Queries/GetInventoryTransactions.cs` |
| Controller | `backend/src/SMS.API/Controllers/InventoryController.cs` |
| Frontend Page | `frontend/src/pages/InventoryManagementPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (inventoryApi section) |

---

## Business Rules
- Transaction "In" increases TotalQuantity, "Out" decreases it
- Items below ReorderLevel should trigger alerts
- Server-side pagination for items and transactions
