using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SMS.Application.Features.Inventory.Commands;
using SMS.Application.Features.Inventory.Queries;
using SMS.Application.Features.Inventory.DTOs;

namespace SMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<InventorySummaryDto>> GetSummary()
    {
        return await _mediator.Send(new GetInventorySummaryQuery());
    }

    [HttpGet("items")]
    public async Task<ActionResult<PaginatedInventoryItemListDto>> GetItems([FromQuery] Guid? categoryId, [FromQuery] bool onlyLowStock = false, [FromQuery] string? searchQuery = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return await _mediator.Send(new GetInventoryItemsQuery 
        { 
            CategoryId = categoryId, 
            OnlyLowStock = onlyLowStock, 
            SearchQuery = searchQuery, 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        });
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<InventoryCategoryDto>>> GetCategories()
    {
        return await _mediator.Send(new GetInventoryCategoriesQuery());
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PaginatedInventoryTransactionListDto>> GetTransactions([FromQuery] Guid? itemId, [FromQuery] string? searchQuery = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return await _mediator.Send(new GetInventoryTransactionsQuery 
        { 
            ItemId = itemId, 
            SearchQuery = searchQuery, 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        });
    }

    [HttpPost("items")]
    public async Task<ActionResult<Guid>> AddItem(AddInventoryItemCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<Guid>> CreateCategory(CreateInventoryCategoryCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<bool>> ProcessTransaction(ProcessStockTransactionCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("categories/{id}")]
    public async Task<ActionResult<bool>> UpdateCategory(Guid id, UpdateInventoryCategoryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }
        return await _mediator.Send(command);
    }
}
