using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Inventory.Commands;

public record CreateInventoryCategoryCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class CreateInventoryCategoryCommandHandler : IRequestHandler<CreateInventoryCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new InventoryCategory
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.InventoryCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
