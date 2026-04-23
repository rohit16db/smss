using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Inventory.DTOs;

namespace SMS.Application.Features.Inventory.Queries;

public record GetInventorySummaryQuery : IRequest<InventorySummaryDto>;

public class GetInventorySummaryQueryHandler : IRequestHandler<GetInventorySummaryQuery, InventorySummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetInventorySummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventorySummaryDto> Handle(GetInventorySummaryQuery request, CancellationToken cancellationToken)
    {
        var totalCategories = await _context.InventoryCategories.CountAsync(cancellationToken);
        var totalItems = await _context.InventoryItems.CountAsync(cancellationToken);
        var lowStockItemsCount = await _context.InventoryItems
            .CountAsync(i => i.TotalQuantity <= i.ReorderLevel, cancellationToken);
        
        var totalInventoryValue = await _context.InventoryItems
            .SumAsync(i => i.TotalQuantity * i.UnitPrice, cancellationToken);

        return new InventorySummaryDto
        {
            TotalCategories = totalCategories,
            TotalItems = totalItems,
            LowStockItemsCount = lowStockItemsCount,
            TotalInventoryValue = totalInventoryValue
        };
    }
}
