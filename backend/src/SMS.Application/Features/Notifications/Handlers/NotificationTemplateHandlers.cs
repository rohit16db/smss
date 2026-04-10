using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Notifications.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Notifications.Handlers;

// --- QUERIES ---

public class GetNotificationTemplatesQuery : IRequest<List<NotificationTemplateDto>>
{
    public string? Category { get; set; }
}

public class GetNotificationTemplateByIdQuery : IRequest<NotificationTemplateDto?>
{
    public Guid Id { get; set; }
}

// --- COMMANDS ---

public class CreateNotificationTemplateCommand : IRequest<NotificationTemplateDto>
{
    public CreateNotificationTemplateDto Dto { get; set; } = null!;
}

public class UpdateNotificationTemplateCommand : IRequest<NotificationTemplateDto>
{
    public UpdateNotificationTemplateDto Dto { get; set; } = null!;
}

public class DeleteNotificationTemplateCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

// --- HANDLERS ---

public class NotificationTemplateHandlers : 
    IRequestHandler<GetNotificationTemplatesQuery, List<NotificationTemplateDto>>,
    IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateDto?>,
    IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplateDto>,
    IRequestHandler<UpdateNotificationTemplateCommand, NotificationTemplateDto>,
    IRequestHandler<DeleteNotificationTemplateCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public NotificationTemplateHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationTemplateDto>> Handle(GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        return await query
            .Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Content = t.Content,
                Channel = t.Channel,
                Category = t.Category,
                IsActive = t.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationTemplateDto?> Handle(GetNotificationTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.NotificationTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (t == null) return null;

        return new NotificationTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Content = t.Content,
            Channel = t.Channel,
            Category = t.Category,
            IsActive = t.IsActive
        };
    }

    public async Task<NotificationTemplateDto> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new NotificationTemplate
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Content = request.Dto.Content,
            Channel = request.Dto.Channel,
            Category = request.Dto.Category,
            IsActive = true
        };

        _context.NotificationTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationTemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Content = entity.Content,
            Channel = entity.Channel,
            Category = entity.Category,
            IsActive = entity.IsActive
        };
    }

    public async Task<NotificationTemplateDto> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Dto.Id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Template with ID {request.Dto.Id} not found");

        entity.Name = request.Dto.Name;
        entity.Description = request.Dto.Description;
        entity.Content = request.Dto.Content;
        entity.Channel = request.Dto.Channel;
        entity.Category = request.Dto.Category;
        entity.IsActive = request.Dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationTemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Content = entity.Content,
            Channel = entity.Channel,
            Category = entity.Category,
            IsActive = entity.IsActive
        };
    }

    public async Task<bool> Handle(DeleteNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.NotificationTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        _context.NotificationTemplates.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
