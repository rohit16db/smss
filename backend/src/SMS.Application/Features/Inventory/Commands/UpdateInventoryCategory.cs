using MediatR;
using SMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SMS.Application.Features.Inventory.Commands;

public record UpdateInventoryCategoryCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class UpdateInventoryCategoryCommandHandler : IRequestHandler<UpdateInventoryCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.InventoryCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return false;
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
