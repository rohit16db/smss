using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Inventory.DTOs;

namespace SMS.Application.Features.Inventory.Queries;

public record GetInventoryItemsQuery : IRequest<PaginatedInventoryItemListDto>
{
    public Guid? CategoryId { get; init; }
    public bool OnlyLowStock { get; init; }
    public string? SearchQuery { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetInventoryItemsQueryHandler : IRequestHandler<GetInventoryItemsQuery, PaginatedInventoryItemListDto>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedInventoryItemListDto> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryItems
            .Include(i => i.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);
        }

        if (request.OnlyLowStock)
        {
            query = query.Where(i => i.TotalQuantity <= i.ReorderLevel);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(search) || i.SKU.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InventoryItemDto
            {
                Id = i.Id.ToString(),
                Name = i.Name,
                SKU = i.SKU,
                Description = i.Description,
                CategoryId = i.CategoryId.ToString(),
                CategoryName = i.Category != null ? i.Category.Name : "Uncategorized",
                TotalQuantity = i.TotalQuantity,
                ReorderLevel = i.ReorderLevel,
                UnitPrice = i.UnitPrice,
                IsActive = i.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PaginatedInventoryItemListDto
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
