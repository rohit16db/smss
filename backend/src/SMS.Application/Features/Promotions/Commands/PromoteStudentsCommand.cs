using MediatR;
using SMS.Application.Features.Promotions.DTOs;

namespace SMS.Application.Features.Promotions.Commands;

public class PromoteStudentsCommand : IRequest<PromotionResultDto>
{
    public required Guid SourceAcademicYearId { get; set; }
    public required Guid TargetAcademicYearId { get; set; }
    public required List<Guid> StudentIds { get; set; }
    public required Guid TargetClassId { get; set; }
    public Guid? TargetSectionId { get; set; }
    public bool MarkSourceAsPromoted { get; set; } = true;
}
