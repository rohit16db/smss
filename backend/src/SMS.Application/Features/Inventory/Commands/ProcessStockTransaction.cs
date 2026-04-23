using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Inventory.Commands;

public record ProcessStockTransactionCommand : IRequest<bool>
{
    public Guid ItemId { get; init; }
    public string TransactionType { get; init; } = "StockIn"; // StockIn or StockOut
    public int Quantity { get; init; }
    public string? ReceivedBy { get; init; }
    public string? Remarks { get; init; }
    public Guid AcademicYearId { get; init; }
}

public class ProcessStockTransactionCommandHandler : IRequestHandler<ProcessStockTransactionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ProcessStockTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ProcessStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null) return false;

        var transaction = new InventoryTransaction
        {
            ItemId = request.ItemId,
            TransactionType = request.TransactionType,
            Quantity = request.Quantity,
            TransactionDate = DateTime.UtcNow,
            ReceivedBy = request.ReceivedBy,
            Remarks = request.Remarks,
            AcademicYearId = request.AcademicYearId
        };

        if (request.TransactionType == "StockIn")
        {
            item.TotalQuantity += request.Quantity;
        }
        else if (request.TransactionType == "StockOut")
        {
            // Optional: Check if enough stock exists
            if (item.TotalQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock to process the request.");
            }
            item.TotalQuantity -= request.Quantity;
        }

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
