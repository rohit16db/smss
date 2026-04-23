using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Inventory.Commands;

public record AddInventoryItemCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CategoryId { get; init; }
    public int InitialQuantity { get; init; }
    public int ReorderLevel { get; init; }
    public decimal UnitPrice { get; init; }
    public Guid AcademicYearId { get; init; }
}

public class AddInventoryItemCommandHandler : IRequestHandler<AddInventoryItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddInventoryItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            CategoryId = request.CategoryId,
            TotalQuantity = request.InitialQuantity,
            ReorderLevel = request.ReorderLevel,
            UnitPrice = request.UnitPrice
        };

        _context.InventoryItems.Add(item);

        // Record initial stock as a transaction if quantity > 0
        if (request.InitialQuantity > 0)
        {
            var transaction = new InventoryTransaction
            {
                ItemId = item.Id,
                TransactionType = "StockIn",
                Quantity = request.InitialQuantity,
                TransactionDate = DateTime.UtcNow,
                Remarks = "Opening Stock",
                AcademicYearId = request.AcademicYearId
            };
            _context.InventoryTransactions.Add(transaction);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
