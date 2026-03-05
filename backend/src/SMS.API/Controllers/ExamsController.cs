using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using SMS.API.Extensions;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Exam Management API endpoints
/// Single Responsibility: Handle HTTP requests for exam management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AcademicAccess")]
public class ExamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(IMediator mediator, ILogger<ExamsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Exam CRUD Endpoints

    /// <summary>
    /// Create a new exam
    /// POST /api/v1/exams
    /// </summary>
    /// <param name="request">Exam creation data</param>
    /// <returns>Created exam</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamDto request)
    {
        try
        {
            var command = new CreateExamCommand
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalMarks = request.TotalMarks,
                PassMarks = request.PassMarks,
                Subjects = request.Subjects.Select(s => new ExamSubjectInput
                {
                    SubjectId = s.SubjectId,
                    MaxMarks = s.MaxMarks,
                    PassMarks = s.PassMarks
                }).ToList(),
                ClassIds = request.ClassIds,
                CreatedById = User.GetCurrentUserId() // Get from JWT token
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetExamById), new { examId = result.Id }, result);
        }
        catch (ValidationException validationEx)
        {
            _logger.LogWarning(validationEx, "Validation error creating exam: {Errors}", string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new {
                message = "Validation failed",
                errors = validationEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                )
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when creating exam: {Message}", ex.Message);
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { 
                message = "Error creating exam",
                error = ex.GetType().Name + ": " + ex.Message
            });
        }
    }

    /// <summary>
    /// Get all exams with filtering and pagination
    /// GET /api/v1/exams
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ExamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams(
        [FromQuery] string? status = null,
        [FromQuery] Guid? classId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetExamsQuery
            {
                Status = status,
                ClassId = classId,
                SubjectId = subjectId,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exams");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving exams" });
        }
    }

    /// <summary>
    /// Get exam details by ID
    /// GET /api/v1/exams/{examId}
    /// </summary>
    [HttpGet("{examId}")]
    [ProducesResponseType(typeof(ExamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamById([FromRoute] Guid examId)
    {
        try
        {
            var query = new GetExamByIdQuery { ExamId = examId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exam");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving exam" });
        }
    }

    /// <summary>
    /// Update exam (only if Draft)
    /// PUT /api/v1/exams/{examId}
    /// </summary>
    [HttpPut("{examId}")]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExam([FromRoute] Guid examId, [FromBody] CreateExamDto request)
    {
        try
        {
            var command = new UpdateExamCommand
            {
                ExamId = examId,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalMarks = request.TotalMarks,
                PassMarks = request.PassMarks,
                Subjects = request.Subjects.Select(s => new ExamSubjectInput 
                { 
                    SubjectId = s.SubjectId, 
                    MaxMarks = s.MaxMarks,
                    PassMarks = s.PassMarks
                }).ToList(),
                ClassIds = request.ClassIds
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exam");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating exam" });
        }
    }

    /// <summary>
    /// Delete/Archive exam (only if Draft)
    /// DELETE /api/v1/exams/{examId}
    /// </summary>
    [HttpDelete("{examId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExam([FromRoute] Guid examId)
    {
        try
        {
            var command = new DeleteExamCommand { ExamId = examId };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Return 400 for validation errors (draft status, marks exist, etc.)
            _logger.LogWarning(ex, "Delete exam validation failed for exam {ExamId}", examId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exam {ExamId}", examId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting exam" });
        }
    }

    /// <summary>
    /// Publish exam (enables marks entry)
    /// POST /api/v1/exams/{examId}/publish
    /// </summary>
    [HttpPost("{examId}/publish")]
    [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishExam([FromRoute] Guid examId)
    {
        try
        {
            var command = new PublishExamCommand { ExamId = examId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing exam");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error publishing exam" });
        }
    }

    #endregion
}
