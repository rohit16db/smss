using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Promotions.Commands;
using SMS.Application.Features.Promotions.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Promotions.Handlers;

public class PromoteStudentsCommandHandler : IRequestHandler<PromoteStudentsCommand, PromotionResultDto>
{
    private readonly IApplicationDbContext _context;

    public PromoteStudentsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PromotionResultDto> Handle(PromoteStudentsCommand request, CancellationToken cancellationToken)
    {
        var result = new PromotionResultDto { Success = true };

        // 1. Get Source Enrollments
        var sourceEnrollments = await _context.Enrollments
            .Where(e => request.StudentIds.Contains(e.StudentId) && 
                        e.AcademicYearId == request.SourceAcademicYearId &&
                        e.Status == "Enrolled")
            .ToListAsync(cancellationToken);

        if (!sourceEnrollments.Any())
        {
            result.Success = false;
            result.Message = "No active enrollments found for the selected students in the source year.";
            return result;
        }

        // 2. Validate Target Year exists
        var targetYearExists = await _context.AcademicYears.AnyAsync(ay => ay.Id == request.TargetAcademicYearId, cancellationToken);
        if (!targetYearExists)
        {
            result.Success = false;
            result.Message = "Target academic year not found.";
            return result;
        }

        // 3. Promote Students
        int count = 0;
        foreach (var sourceEnrollment in sourceEnrollments)
        {
            // Check if student already has an enrollment in the target year
            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => 
                e.StudentId == sourceEnrollment.StudentId && 
                e.AcademicYearId == request.TargetAcademicYearId, cancellationToken);
            
            if (alreadyEnrolled)
            {
                result.Errors.Add($"Student with ID {sourceEnrollment.StudentId} already enrolled in target year.");
                continue;
            }

            // Create new enrollment
            var newEnrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = sourceEnrollment.StudentId,
                AcademicYearId = request.TargetAcademicYearId,
                ClassId = request.TargetClassId,
                SectionId = request.TargetSectionId,
                Status = "Enrolled",
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(newEnrollment);

            // Update source enrollment status
            if (request.MarkSourceAsPromoted)
            {
                sourceEnrollment.Status = "Promoted";
            }
            
            count++;
        }

        if (count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            result.PromotedCount = count;
            result.Message = $"Successfully promoted {count} students.";
        }
        else if (result.Errors.Any())
        {
            result.Success = false;
            result.Message = "Promotion failed for all selected students.";
        }

        return result;
    }
}
