using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Extensions;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Marks Management API endpoints
/// Single Responsibility: Handle HTTP requests for marks management
/// </summary>
[ApiController]
[Route("api/v1/exams/{examId}/marks")]
[Authorize(Policy = "AcademicAccess")]
public class MarksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MarksController> _logger;

    public MarksController(IMediator mediator, ILogger<MarksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Marks Entry Endpoints

    /// <summary>
    /// Get marks entry form for a class
    /// GET /api/v1/exams/{examId}/marks/form/{classId}
    /// </summary>
    [HttpGet("form/{classId}")]
    [ProducesResponseType(typeof(MarksEntryFormDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMarksEntryForm([FromRoute] Guid examId, [FromRoute] Guid classId, [FromQuery] Guid? sectionId, [FromQuery] string sortBy = "rollNumber")
    {
        try
        {
            var query = new GetMarksEntryFormQuery
            {
                ExamId = examId,
                ClassId = classId,
                SectionId = sectionId ?? Guid.Empty,
                SortBy = sortBy
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marks entry form");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving marks entry form" });
        }
    }

    /// <summary>
    /// Save student marks (Draft mode)
    /// POST /api/v1/exams/{examId}/marks/save/{classId}
    /// </summary>
    [HttpPost("save/{classId}")]
    [ProducesResponseType(typeof(SaveMarksResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveMarks([FromRoute] Guid examId, [FromRoute] Guid classId, [FromQuery] Guid sectionId, [FromBody] List<StudentMarksEntryDto> marksData)
    {
        try
        {
            var command = new SaveStudentMarksCommand
            {
                ExamId = examId,
                ClassId = classId,
                SectionId = sectionId,
                MarksData = marksData
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving marks");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error saving marks" });
        }
    }

    /// <summary>
    /// Get marks for a single student
    /// GET /api/v1/exams/{examId}/marks/student/{studentId}
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(StudentMarksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentMarks([FromRoute] Guid examId, [FromRoute] Guid studentId)
    {
        try
        {
            var query = new GetStudentMarksQuery
            {
                ExamId = examId,
                StudentId = studentId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student marks");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving student marks" });
        }
    }

    /// <summary>
    /// Get marks for all students in a class
    /// GET /api/v1/exams/{examId}/marks/class/{classId}
    /// </summary>
    [HttpGet("class/{classId}")]
    [ProducesResponseType(typeof(List<StudentMarksDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassMarks([FromRoute] Guid examId, [FromRoute] Guid classId)
    {
        try
        {
            var query = new GetClassMarksQuery
            {
                ExamId = examId,
                ClassId = classId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class marks");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving class marks" });
        }
    }

    /// <summary>
    /// Submit marks for a class (Finalize marks)
    /// POST /api/v1/exams/{examId}/marks/submit/{classId}
    /// </summary>
    [HttpPost("submit/{classId}")]
    [ProducesResponseType(typeof(SaveMarksResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitMarks([FromRoute] Guid examId, [FromRoute] Guid classId, [FromQuery] Guid sectionId)
    {
        try
        {
            var command = new SubmitMarksCommand
            {
                ExamId = examId,
                ClassId = classId,
                SectionId = sectionId,
                ConfirmedById = User.GetCurrentUserId() // Get from JWT token
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting marks");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error submitting marks" });
        }
    }

    #endregion
}
