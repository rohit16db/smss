namespace SMS.Application.Features.Inventory.DTOs;

public class InventoryCategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
}

public class InventoryItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

public class InventoryTransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Remarks { get; set; }
}

public class InventorySummaryDto
{
    public int TotalCategories { get; set; }
    public int TotalItems { get; set; }
    public int LowStockItemsCount { get; set; }
    public int TotalStockValue { get; set; } // Simplified to count for now or decimal? Let's use decimal for value.
    public decimal TotalInventoryValue { get; set; }
}

public class PaginatedInventoryItemListDto
{
    public List<InventoryItemDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

public class PaginatedInventoryTransactionListDto
{
    public List<InventoryTransactionDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
