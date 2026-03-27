using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Features.Attendance.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Attendance Management API endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AttendanceAccess")]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IMediator mediator, ILogger<AttendanceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Student Attendance Endpoints

    /// <summary>
    /// Mark student attendance
    /// Section is auto-detected from student's current enrollment
    /// </summary>
    /// <param name="dto">Student attendance data</param>
    /// <returns>Created attendance record</returns>
    [HttpPost("students")]
    [ProducesResponseType(typeof(StudentAttendanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkStudentAttendance([FromBody] MarkStudentAttendanceDto dto)
    {
        try
        {
            var command = new MarkStudentAttendanceCommand
            {
                StudentId = dto.StudentId,
                // SectionId removed - auto-detected from student enrollment
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status,
                Reason = dto.Reason,
                CreatedByUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStudentAttendanceById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid student attendance request");
            // Check if it's a duplicate attendance error
            if (ex.Message.Contains("already marked", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking student attendance");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error marking student attendance");
        }
    }

    /// <summary>
    /// Get student attendance by ID
    /// </summary>
    /// <param name="id">Attendance ID (GUID)</param>
    /// <returns>Student attendance details</returns>
    [HttpGet("students/{id}")]
    [ProducesResponseType(typeof(StudentAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentAttendanceById(string id)
    {
        try
        {
            var query = new GetStudentAttendanceByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Student attendance with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student attendance");
        }
    }

    /// <summary>
    /// Get student attendance by date and section
    /// </summary>
    /// <param name="sectionId">Section ID (GUID)</param>
    /// <param name="date">Attendance date</param>
    /// <returns>List of student attendance records</returns>
    [HttpGet("students/by-date")]
    [ProducesResponseType(typeof(List<StudentAttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAttendanceByDate([FromQuery] string sectionId, [FromQuery] DateTime date)
    {
        try
        {
            var query = new GetStudentAttendanceByDateQuery
            {
                SectionId = sectionId,
                AttendanceDate = date
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student attendance for section {SectionId} on {Date}", sectionId, date);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student attendance");
        }
    }

    /// <summary>
    /// Get student attendance history with pagination and filtering
    /// </summary>
    /// <param name="studentId">Filter by student ID</param>
    /// <param name="sectionId">Filter by section ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="status">Filter by status</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of student attendance records</returns>
    [HttpGet("students/history")]
    [ProducesResponseType(typeof(PaginatedStudentAttendanceListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAttendanceHistory(
        [FromQuery] string? studentId = null,
        [FromQuery] string? sectionId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetStudentAttendanceHistoryQuery
            {
                StudentId = studentId,
                SectionId = sectionId,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student attendance history");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student attendance history");
        }
    }

    /// <summary>
    /// Get student attendance summary statistics
    /// </summary>
    /// <param name="studentId">Student ID (GUID)</param>
    /// <param name="startDate">Start date for summary</param>
    /// <param name="endDate">End date for summary</param>
    /// <returns>Attendance summary</returns>
    [HttpGet("students/{studentId}/summary")]
    [ProducesResponseType(typeof(AttendanceStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentAttendanceSummary(
        string studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = new GetStudentAttendanceSummaryQuery
            {
                StudentId = studentId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid student ID: {StudentId}", studentId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student attendance summary for {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student attendance summary");
        }
    }

    /// <summary>
    /// Update student attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <param name="dto">Updated attendance data</param>
    /// <returns>Updated attendance record</returns>
    [HttpPut("students/{id}")]
    [ProducesResponseType(typeof(StudentAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudentAttendance(string id, [FromBody] UpdateStudentAttendanceDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var command = new UpdateStudentAttendanceCommand
            {
                Id = dto.Id,
                Status = dto.Status,
                Reason = dto.Reason,
                UpdatedByUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Student attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating student attendance");
        }
    }

    /// <summary>
    /// Delete student attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("students/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudentAttendance(string id)
    {
        try
        {
            var command = new DeleteStudentAttendanceCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Student attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting student attendance");
        }
    }

    #endregion

    #region Staff Attendance Endpoints

    /// <summary>
    /// Record staff attendance
    /// </summary>
    /// <param name="dto">Staff attendance data</param>
    /// <returns>Created attendance record</returns>
    [HttpPost("staff")]
    [ProducesResponseType(typeof(StaffAttendanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordStaffAttendance([FromBody] RecordStaffAttendanceDto dto)
    {
        try
        {
            var command = new RecordStaffAttendanceCommand
            {
                StaffId = dto.StaffId,
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status,
                Reason = dto.Reason,
                CreatedByUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStaffAttendanceById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid staff attendance request");
            // Check if it's a duplicate attendance error
            if (ex.Message.Contains("already marked", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording staff attendance");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error recording staff attendance");
        }
    }

    /// <summary>
    /// Get staff attendance by ID
    /// </summary>
    /// <param name="id">Attendance ID (GUID)</param>
    /// <returns>Staff attendance details</returns>
    [HttpGet("staff/{id}")]
    [ProducesResponseType(typeof(StaffAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffAttendanceById(string id)
    {
        try
        {
            var query = new GetStaffAttendanceByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Staff attendance with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving staff attendance");
        }
    }

    /// <summary>
    /// Get staff attendance by date
    /// </summary>
    /// <param name="date">Attendance date</param>
    /// <returns>List of staff attendance records</returns>
    [HttpGet("staff/by-date")]
    [ProducesResponseType(typeof(List<StaffAttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffAttendanceByDate([FromQuery] DateTime date)
    {
        try
        {
            var query = new GetStaffAttendanceByDateQuery { AttendanceDate = date };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff attendance for date {Date}", date);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving staff attendance");
        }
    }

    /// <summary>
    /// Get staff attendance history with pagination and filtering
    /// </summary>
    /// <param name="staffId">Filter by staff ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="status">Filter by status</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of staff attendance records</returns>
    [HttpGet("staff/history")]
    [ProducesResponseType(typeof(PaginatedStaffAttendanceListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffAttendanceHistory(
        [FromQuery] string? staffId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetStaffAttendanceHistoryQuery
            {
                StaffId = staffId,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff attendance history");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving staff attendance history");
        }
    }

    /// <summary>
    /// Get staff attendance summary statistics
    /// </summary>
    /// <param name="staffId">Staff ID (GUID)</param>
    /// <param name="startDate">Start date for summary</param>
    /// <param name="endDate">End date for summary</param>
    /// <returns>Attendance summary</returns>
    [HttpGet("staff/{staffId}/summary")]
    [ProducesResponseType(typeof(AttendanceStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffAttendanceSummary(
        string staffId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = new GetStaffAttendanceSummaryQuery
            {
                StaffId = staffId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid staff ID: {StaffId}", staffId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff attendance summary for {StaffId}", staffId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving staff attendance summary");
        }
    }

    /// <summary>
    /// Update staff attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <param name="dto">Updated attendance data</param>
    /// <returns>Updated attendance record</returns>
    [HttpPut("staff/{id}")]
    [ProducesResponseType(typeof(StaffAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaffAttendance(string id, [FromBody] UpdateStaffAttendanceDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var command = new UpdateStaffAttendanceCommand
            {
                Id = dto.Id,
                Status = dto.Status,
                Reason = dto.Reason,
                UpdatedByUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Staff attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating staff attendance");
        }
    }

    /// <summary>
    /// Delete staff attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("staff/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStaffAttendance(string id)
    {
        try
        {
            var command = new DeleteStaffAttendanceCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Staff attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting staff attendance");
        }
    }

    #endregion

    /// <summary>
    /// Helper method to get current user ID from claims
    /// </summary>
    private string GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
        return userIdClaim?.Value ?? Guid.Empty.ToString();
    }

    #region Attendance Report Endpoints

    /// <summary>
    /// Get monthly attendance report with pagination
    /// </summary>
    [HttpGet("reports/monthly")]
    [ProducesResponseType(typeof(PaginatedMonthlyAttendanceReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyAttendanceReport(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10, [FromQuery] string? studentId = null, [FromQuery] string? sectionId = null)
    {
        try
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Month must be between 1 and 12" });
            var query = new GetMonthlyAttendanceReportQuery
            { Year = year, Month = month, PageNumber = pageNumber, PageSize = pageSize, StudentId = studentId, SectionId = sectionId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly attendance report");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving attendance report");
        }
    }

    /// <summary>
    /// Get low attendance alerts
    /// </summary>
    [HttpGet("reports/low-attendance")]
    [ProducesResponseType(typeof(List<LowAttendanceAlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowAttendanceAlerts(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] string? sectionId = null, [FromQuery] decimal threshold = 75m)
    {
        try
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Month must be between 1 and 12" });
            if (threshold < 0 || threshold > 100)
                return BadRequest(new { message = "Threshold must be between 0 and 100" });
            var query = new GetLowAttendanceAlertsQuery
            { Year = year, Month = month, SectionId = sectionId, AttendanceThreshold = threshold };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low attendance alerts");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving low attendance alerts");
        }
    }

    /// <summary>
    /// Get class attendance summary
    /// </summary>
    [HttpGet("reports/class-summary")]
    [ProducesResponseType(typeof(List<ClassAttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClassAttendanceSummary(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] string? sectionId = null)
    {
        try
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Month must be between 1 and 12" });
            var query = new GetClassAttendanceSummaryQuery { Year = year, Month = month, SectionId = sectionId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class attendance summary");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving class attendance summary");
        }
    }

    #endregion
}
