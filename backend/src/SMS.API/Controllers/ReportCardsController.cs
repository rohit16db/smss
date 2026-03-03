using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Report Card API endpoints
/// Single Responsibility: Handle HTTP requests for report card management
/// </summary>
[ApiController]
[Route("api/v1/reportcards")]
[Authorize]
public class ReportCardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportCardsController> _logger;

    public ReportCardsController(IMediator mediator, ILogger<ReportCardsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Report Card Endpoints

    /// <summary>
    /// Get all report cards for a specific exam
    /// GET /api/v1/reportcards/exam/{examId}
    /// </summary>
    [HttpGet("exam/{examId}")]
    [ProducesResponseType(typeof(List<ReportCardListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamReportCards(
        [FromRoute] Guid examId,
        [FromQuery] Guid? classId = null,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "classPosition",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetExamReportCardsQuery
            {
                ExamId = examId,
                ClassId = classId,
                Status = status,
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
            _logger.LogError(ex, "Error retrieving exam report cards");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving exam report cards" });
        }
    }

    /// <summary>
    /// Get all report cards for a specific student
    /// GET /api/v1/reportcards/student/{studentId}
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(typeof(List<ReportCardListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentReportCards([FromRoute] Guid studentId)
    {
        try
        {
            var query = new GetStudentReportCardsQuery { StudentId = studentId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student report cards");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving student report cards" });
        }
    }

    /// <summary>
    /// Get report card by ID
    /// GET /api/v1/reportcards/{cardId}
    /// </summary>
    [HttpGet("{cardId:guid}")]
    [ProducesResponseType(typeof(ReportCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportCardById([FromRoute] Guid cardId)
    {
        try
        {
            var query = new GetReportCardByIdQuery
            {
                ReportCardId = cardId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Report card not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report card");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving report card" });
        }
    }

    /// <summary>
    /// Get report card for a specific student and exam
    /// GET /api/v1/reportcards/{examId}/{studentId}
    /// </summary>
    [HttpGet("{examId:guid}/{studentId:guid}")]
    [ProducesResponseType(typeof(ReportCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportCard([FromRoute] Guid examId, [FromRoute] Guid studentId)
    {
        try
        {
            var query = new GetReportCardQuery
            {
                ExamId = examId,
                StudentId = studentId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report card");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving report card" });
        }
    }

    /// <summary>
    /// Export report card as PDF
    /// POST /api/v1/reportcards/{cardId}/export-pdf
    /// </summary>
    [HttpPost("{cardId}/export-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportReportCardPdf([FromRoute] Guid cardId)
    {
        try
        {
            var query = new ExportReportCardPdfQuery { CardId = cardId };
            var pdfBytes = await _mediator.Send(query);
            return File(pdfBytes, "application/pdf", $"report-card-{cardId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Report card not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report card as PDF");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error exporting report card" });
        }
    }

    #endregion
}
