using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Inventory.DTOs;

namespace SMS.Application.Features.Inventory.Queries;

public record GetInventoryCategoriesQuery : IRequest<List<InventoryCategoryDto>>;

public class GetInventoryCategoriesQueryHandler : IRequestHandler<GetInventoryCategoriesQuery, List<InventoryCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryCategoryDto>> Handle(GetInventoryCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categoryCounts = await _context.InventoryItems
            .GroupBy(i => i.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.CategoryId, v => v.Count, cancellationToken);

        var categories = await _context.InventoryCategories
            .Select(c => new InventoryCategoryDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            if (Guid.TryParse(category.Id, out Guid id) && categoryCounts.TryGetValue(id, out int count))
            {
                category.ItemCount = count;
            }
        }

        return categories;
    }
}
