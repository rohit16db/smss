using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// API Controller for Exam Analytics & Reporting
/// Single Responsibility: Handle analytics endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IMediator mediator, ILogger<AnalyticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get exam performance analytics with pass rates, averages, grade distribution
    /// GET /api/analytics/exams/{examId}
    /// </summary>
    [HttpGet("exams/{examId}")]
    [ProducesResponseType(typeof(ExamAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExamAnalyticsDto>> GetExamAnalytics(
        Guid examId,
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting analytics for exam {ExamId}", examId);

            var query = new GetExamAnalyticsQuery
            {
                ExamId = examId,
                ClassId = classId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam analytics for {ExamId}", examId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving analytics" });
        }
    }

    /// <summary>
    /// Get detailed class performance metrics for exam
    /// GET /api/analytics/classes/{classId}/exams/{examId}
    /// </summary>
    [HttpGet("classes/{classId}/exams/{examId}")]
    [ProducesResponseType(typeof(ClassPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassPerformanceDto>> GetClassPerformance(
        Guid classId,
        Guid examId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting class performance for class {ClassId} exam {ExamId}", classId, examId);

            var query = new GetClassPerformanceQuery
            {
                ExamId = examId,
                ClassId = classId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting class performance");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving class performance" });
        }
    }

    /// <summary>
    /// Get student performance trend across exams
    /// GET /api/analytics/students/{studentId}/trend
    /// </summary>
    [HttpGet("students/{studentId}/trend")]
    [ProducesResponseType(typeof(StudentPerformanceTrendDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentPerformanceTrendDto>> GetStudentPerformanceTrend(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting performance trend for student {StudentId}", studentId);

            var query = new GetStudentPerformanceTrendQuery
            {
                StudentId = studentId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student performance trend");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving performance trend" });
        }
    }

    /// <summary>
    /// Compare class performance across multiple classes for same exam
    /// GET /api/analytics/exams/{examId}/class-comparison
    /// </summary>
    [HttpGet("exams/{examId}/class-comparison")]
    [ProducesResponseType(typeof(ClassComparativeAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassComparativeAnalysisDto>> GetClassComparison(
        Guid examId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting class comparison for exam {ExamId}", examId);

            var query = new GetClassComparativeAnalysisQuery
            {
                ExamId = examId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting class comparison");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving class comparison" });
        }
    }

    /// <summary>
    /// Get marks distribution (histogram data) for exam
    /// GET /api/analytics/exams/{examId}/marks-distribution
    /// </summary>
    [HttpGet("exams/{examId}/marks-distribution")]
    [ProducesResponseType(typeof(MarksDistributionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MarksDistributionDto>> GetMarksDistribution(
        Guid examId,
        Guid? classId = null,
        int bucketSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting marks distribution for exam {ExamId}", examId);

            var query = new GetMarksDistributionQuery
            {
                ExamId = examId,
                ClassId = classId,
                BucketSize = bucketSize > 0 ? bucketSize : 10
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting marks distribution");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving marks distribution" });
        }
    }

    /// <summary>
    /// Compare exam performance trends in a class
    /// GET /api/analytics/classes/{classId}/exam-comparison
    /// </summary>
    [HttpGet("classes/{classId}/exam-comparison")]
    [ProducesResponseType(typeof(ExamComparisonAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExamComparisonAnalysisDto>> GetExamComparison(
        Guid classId,
        int? limitToLastN = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting exam comparison for class {ClassId}", classId);

            var query = new GetExamComparisonQuery
            {
                ClassId = classId,
                LimitToLastNExams = limitToLastN
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam comparison");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving exam comparison" });
        }
    }

    /// <summary>
    /// Analyze subject performance across multiple exams
    /// GET /api/analytics/subjects/{subjectId}/comparison
    /// </summary>
    [HttpGet("subjects/{subjectId}/comparison")]
    [ProducesResponseType(typeof(SubjectComparisonAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubjectComparisonAnalysisDto>> GetSubjectComparison(
        Guid subjectId,
        int? limitToLastN = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subject comparison for subject {SubjectId}", subjectId);

            var query = new GetSubjectComparisonQuery
            {
                SubjectId = subjectId,
                LimitToLastNExams = limitToLastN
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject comparison");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving subject comparison" });
        }
    }

    /// <summary>
    /// Generate detailed analytics report for export
    /// GET /api/analytics/reports/detailed
    /// </summary>
    [HttpGet("reports/detailed")]
    [ProducesResponseType(typeof(DetailedAnalyticsReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DetailedAnalyticsReportDto>> GetDetailedReport(
        Guid examId,
        Guid? classId = null,
        DateTime? reportStart = null,
        DateTime? reportEnd = null,
        bool includeStudents = true,
        bool includeSubjects = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating detailed analytics report for exam {ExamId}", examId);

            var query = new GetDetailedAnalyticsReportQuery
            {
                ExamId = examId,
                ClassId = classId,
                ReportPeriodStart = reportStart,
                ReportPeriodEnd = reportEnd,
                IncludeStudentDetails = includeStudents,
                IncludeSubjectAnalysis = includeSubjects
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating detailed report");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report" });
        }
    }

    /// <summary>
    /// Export analytics report as JSON
    /// GET /api/analytics/reports/export-json
    /// </summary>
    [HttpGet("reports/export-json")]
    [Produces("application/json")]
    public async Task<ActionResult> ExportAnalyticsJson(
        Guid examId,
        Guid? classId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting analytics as JSON for exam {ExamId}", examId);

            var query = new GetDetailedAnalyticsReportQuery
            {
                ExamId = examId,
                ClassId = classId,
                IncludeStudentDetails = true,
                IncludeSubjectAnalysis = true
            };

            var report = await _mediator.Send(query, cancellationToken);

            var fileName = $"analytics-{examId:N}-{DateTime.UtcNow:yyyyMMdd}.json";
            return File(
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(report),
                "application/json",
                fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error exporting analytics" });
        }
    }
}
