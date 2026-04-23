using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Inventory.DTOs;

namespace SMS.Application.Features.Inventory.Queries;

public record GetInventoryTransactionsQuery : IRequest<PaginatedInventoryTransactionListDto>
{
    public Guid? ItemId { get; init; }
    public string? SearchQuery { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetInventoryTransactionsQueryHandler : IRequestHandler<GetInventoryTransactionsQuery, PaginatedInventoryTransactionListDto>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedInventoryTransactionListDto> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Item)
            .AsQueryable();

        if (request.ItemId.HasValue)
        {
            query = query.Where(t => t.ItemId == request.ItemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(t => t.Item != null && (t.Item.Name.ToLower().Contains(search) || t.Item.SKU.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new InventoryTransactionDto
            {
                Id = t.Id.ToString(),
                ItemId = t.ItemId.ToString(),
                ItemName = t.Item != null ? t.Item.Name : "Unknown Item",
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                TransactionDate = t.TransactionDate,
                ReceivedBy = t.ReceivedBy,
                Remarks = t.Remarks
            })
            .ToListAsync(cancellationToken);

        return new PaginatedInventoryTransactionListDto
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
