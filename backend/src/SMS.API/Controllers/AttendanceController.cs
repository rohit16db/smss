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

    #region Teacher Attendance Endpoints

    /// <summary>
    /// Record teacher attendance
    /// </summary>
    /// <param name="dto">Teacher attendance data</param>
    /// <returns>Created attendance record</returns>
    [HttpPost("teachers")]
    [ProducesResponseType(typeof(TeacherAttendanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordTeacherAttendance([FromBody] RecordTeacherAttendanceDto dto)
    {
        try
        {
            var command = new RecordTeacherAttendanceCommand
            {
                TeacherId = dto.TeacherId,
                AttendanceDate = dto.AttendanceDate,
                Status = dto.Status,
                Reason = dto.Reason,
                CreatedByUserId = GetCurrentUserId()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTeacherAttendanceById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid teacher attendance request");
            // Check if it's a duplicate attendance error
            if (ex.Message.Contains("already marked", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording teacher attendance");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error recording teacher attendance");
        }
    }

    /// <summary>
    /// Get teacher attendance by ID
    /// </summary>
    /// <param name="id">Attendance ID (GUID)</param>
    /// <returns>Teacher attendance details</returns>
    [HttpGet("teachers/{id}")]
    [ProducesResponseType(typeof(TeacherAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacherAttendanceById(string id)
    {
        try
        {
            var query = new GetTeacherAttendanceByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Teacher attendance with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving teacher attendance");
        }
    }

    /// <summary>
    /// Get teacher attendance by date
    /// </summary>
    /// <param name="date">Attendance date</param>
    /// <returns>List of teacher attendance records</returns>
    [HttpGet("teachers/by-date")]
    [ProducesResponseType(typeof(List<TeacherAttendanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherAttendanceByDate([FromQuery] DateTime date)
    {
        try
        {
            var query = new GetTeacherAttendanceByDateQuery { AttendanceDate = date };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher attendance for date {Date}", date);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving teacher attendance");
        }
    }

    /// <summary>
    /// Get teacher attendance history with pagination and filtering
    /// </summary>
    /// <param name="teacherId">Filter by teacher ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="status">Filter by status</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of teacher attendance records</returns>
    [HttpGet("teachers/history")]
    [ProducesResponseType(typeof(PaginatedTeacherAttendanceListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherAttendanceHistory(
        [FromQuery] string? teacherId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetTeacherAttendanceHistoryQuery
            {
                TeacherId = teacherId,
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
            _logger.LogError(ex, "Error retrieving teacher attendance history");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving teacher attendance history");
        }
    }

    /// <summary>
    /// Get teacher attendance summary statistics
    /// </summary>
    /// <param name="teacherId">Teacher ID (GUID)</param>
    /// <param name="startDate">Start date for summary</param>
    /// <param name="endDate">End date for summary</param>
    /// <returns>Attendance summary</returns>
    [HttpGet("teachers/{teacherId}/summary")]
    [ProducesResponseType(typeof(AttendanceStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherAttendanceSummary(
        string teacherId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = new GetTeacherAttendanceSummaryQuery
            {
                TeacherId = teacherId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid teacher ID: {TeacherId}", teacherId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher attendance summary for {TeacherId}", teacherId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving teacher attendance summary");
        }
    }

    /// <summary>
    /// Update teacher attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <param name="dto">Updated attendance data</param>
    /// <returns>Updated attendance record</returns>
    [HttpPut("teachers/{id}")]
    [ProducesResponseType(typeof(TeacherAttendanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeacherAttendance(string id, [FromBody] UpdateTeacherAttendanceDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var command = new UpdateTeacherAttendanceCommand
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
            _logger.LogWarning(ex, "Teacher attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating teacher attendance");
        }
    }

    /// <summary>
    /// Delete teacher attendance record
    /// </summary>
    /// <param name="id">Attendance ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("teachers/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeacherAttendance(string id)
    {
        try
        {
            var command = new DeleteTeacherAttendanceCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Teacher attendance not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher attendance with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting teacher attendance");
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
    }}