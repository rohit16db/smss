using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Extensions;
using SMS.Application.Features.Fees.Commands;
using SMS.Application.Features.Fees.DTOs;
using SMS.Application.Features.Fees.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Fee Management API endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "FeesAccess")]
public class FeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FeesController> _logger;

    public FeesController(IMediator mediator, ILogger<FeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Fee Structure Endpoints

    /// <summary>
    /// Get all fee structures with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Search by name</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="academicYear">Filter by academic year</param>
    /// <returns>Paginated list of fee structures</returns>
    [HttpGet("structures")]
    [ProducesResponseType(typeof(PaginatedFeeStructureListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeeStructures(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? academicYearId = null)
    {
        try
        {
            var query = new GetAllFeeStructuresQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                IsActive = isActive,
                AcademicYearId = academicYearId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee structures");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving fee structures");
        }
    }

    /// <summary>
    /// Get active fee structures (for dropdowns)
    /// </summary>
    /// <param name="academicYear">Optional academic year filter</param>
    /// <returns>List of active fee structures</returns>
    [HttpGet("structures/active")]
    [ProducesResponseType(typeof(List<FeeStructureListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveFeeStructures([FromQuery] string? academicYearId = null)
    {
        try
        {
            var query = new GetActiveFeeStructuresQuery { AcademicYearId = academicYearId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active fee structures");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving active fee structures");
        }
    }

    /// <summary>
    /// Get fee structure by ID
    /// </summary>
    /// <param name="id">Fee structure ID (GUID)</param>
    /// <returns>Fee structure details</returns>
    [HttpGet("structures/{id}")]
    [ProducesResponseType(typeof(FeeStructureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeeStructureById(string id)
    {
        try
        {
            var query = new GetFeeStructureByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Fee structure with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee structure with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving fee structure");
        }
    }

    /// <summary>
    /// Create a new fee structure
    /// </summary>
    /// <param name="command">Fee structure creation data</param>
    /// <returns>Created fee structure</returns>
    [HttpPost("structures")]
    [ProducesResponseType(typeof(FeeStructureDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFeeStructure([FromBody] CreateFeeStructureDto dto)
    {
        try
        {
            var command = new CreateFeeStructureCommand
            {
                Name = dto.Name,
                AcademicYearId = dto.AcademicYearId,
                Frequency = dto.Frequency,
                Categories = dto.Categories,
                CreatedByUserId = User.GetCurrentUserId().ToString()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetFeeStructureById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fee structure");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating fee structure");
        }
    }

    /// <summary>
    /// Update an existing fee structure
    /// </summary>
    /// <param name="id">Fee structure ID</param>
    /// <param name="dto">Updated fee structure data</param>
    /// <returns>Updated fee structure</returns>
    [HttpPut("structures/{id}")]
    [ProducesResponseType(typeof(FeeStructureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeeStructure(string id, [FromBody] UpdateFeeStructureDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var command = new UpdateFeeStructureCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                AcademicYearId = dto.AcademicYearId,
                Frequency = dto.Frequency,
                IsActive = dto.IsActive,
                Categories = dto.Categories,
                UpdatedByUserId = User.GetCurrentUserId().ToString()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fee structure not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee structure with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating fee structure");
        }
    }

    /// <summary>
    /// Delete a fee structure
    /// </summary>
    /// <param name="id">Fee structure ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("structures/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeeStructure(string id)
    {
        try
        {
            var command = new DeleteFeeStructureCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Fee structure not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee structure with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting fee structure");
        }
    }

    #endregion

    #region Student Fee Endpoints
    /// <summary>
    /// Get all student fees with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="studentId">Optional filter by student ID</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <returns>Paginated list of student fees</returns>
    [HttpGet("student-fees")]
    [ProducesResponseType(typeof(PaginatedStudentFeeListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStudentFees(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? studentId = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetAllStudentFeesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                StudentId = studentId,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student fees");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student fees");
        }
    }

    /// <summary>
    /// Get student fees by student ID
    /// </summary>
    /// <param name="studentId">Student ID (GUID)</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>List of student fees</returns>
    [HttpGet("student-fees/student/{studentId}")]
    [ProducesResponseType(typeof(List<StudentFeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentFeesByStudentId(string studentId, [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetStudentFeesByStudentIdQuery
            {
                StudentId = studentId,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student fees for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student fees");
        }
    }

    /// <summary>
    /// Get student fees by section ID
    /// Shows all students in a section and their fee status
    /// </summary>
    /// <param name="sectionId">Section ID (GUID)</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>List of student fees for the section</returns>
    [HttpGet("student-fees/section/{sectionId}")]
    [ProducesResponseType(typeof(List<StudentFeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentFeesBySection(string sectionId, [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetFeesBySectionQuery
            {
                SectionId = sectionId,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student fees for section {SectionId}", sectionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student fees for section");
        }
    }

    /// <summary>
    /// Get student fee by ID
    /// </summary>
    /// <param name="id">Student fee ID (GUID)</param>
    /// <returns>Student fee details</returns>
    [HttpGet("student-fees/{id}")]
    [ProducesResponseType(typeof(StudentFeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentFeeById(string id)
    {
        try
        {
            var query = new GetStudentFeeByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Student fee with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student fee with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving student fee");
        }
    }

    /// <summary>
    /// Assign a fee structure to a student
    /// </summary>
    /// <param name="dto">Student fee assignment data</param>
    /// <returns>Created student fee</returns>
    [HttpPost("student-fees")]
    [ProducesResponseType(typeof(StudentFeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignStudentFee([FromBody] AssignStudentFeeDto dto)
    {
        try
        {
            var command = new AssignStudentFeeCommand
            {
                StudentId = dto.StudentId,
                FeeStructureId = dto.FeeStructureId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedByUserId = User.GetCurrentUserId().ToString()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStudentFeeById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid fee assignment request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning student fee: {Message}", ex.Message);
            Console.WriteLine($"Assign Fee ERROR: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error assigning student fee: {ex.Message}");
        }
    }

    /// <summary>
    /// Terminate a student fee assignment
    /// </summary>
    /// <param name="id">Student fee ID (GUID)</param>
    /// <param name="dto">Termination data including end date</param>
    /// <returns>Updated student fee</returns>
    [HttpPatch("student-fees/{id}/terminate")]
    [ProducesResponseType(typeof(StudentFeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateStudentFee(string id, [FromBody] TerminateStudentFeeDto dto)
    {
        try
        {
            var command = new TerminateStudentFeeCommand
            {
                Id = id,
                EndDate = dto.EndDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid fee termination request for ID {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating student fee with ID {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error terminating student fee");
        }
    }

    /// <summary>
    /// Bulk assign fee structure to all students in a section
    /// </summary>
    /// <param name="dto">Bulk assignment data including fee structure, section, and dates</param>
    /// <returns>Result with success/skip/failure counts</returns>
    [HttpPost("student-fees/bulk-assign")]
    [ProducesResponseType(typeof(BulkAssignmentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAssignStudentFee([FromBody] BulkAssignStudentFeeDto dto)
    {
        try
        {
            var command = new BulkAssignStudentFeeCommand
            {
                FeeStructureId = dto.FeeStructureId,
                SectionId = dto.SectionId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                SkipAlreadyAssigned = dto.SkipAlreadyAssigned,
                CreatedByUserId = User.GetCurrentUserId().ToString()
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid bulk fee assignment request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk fee assignment");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error performing bulk fee assignment");
        }
    }

    #endregion

    #region Fee Payment Endpoints

    /// <summary>
    /// Get all fee payments with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="studentFeeId">Filter by student fee ID (optional)</param>
    /// <returns>Paginated list of fee payments</returns>
    [HttpGet("payments")]
    [ProducesResponseType(typeof(PaginatedFeePaymentListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPayments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? studentFeeId = null)
    {
        try
        {
            var query = new GetAllFeePaymentsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                StudentFeeId = studentFeeId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee payments");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving fee payments");
        }
    }

    /// <summary>
    /// Get payment history for a student fee
    /// </summary>
    /// <param name="studentFeeId">Student fee ID (GUID)</param>
    /// <returns>List of fee payments</returns>
    [HttpGet("payments/student-fee/{studentFeeId}")]
    [ProducesResponseType(typeof(List<FeePaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeePaymentsByStudentFeeId(string studentFeeId)
    {
        try
        {
            var query = new GetFeePaymentsByStudentFeeIdQuery { StudentFeeId = studentFeeId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee payments for student fee {StudentFeeId}", studentFeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving fee payments");
        }
    }

    /// <summary>
    /// Record a fee payment
    /// </summary>
    /// <param name="dto">Fee payment data</param>
    /// <returns>Created fee payment</returns>
    [HttpPost("payments")]
    [ProducesResponseType(typeof(FeePaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordFeePayment([FromBody] RecordFeePaymentDto dto)
    {
        try
        {
            var command = new RecordFeePaymentCommand
            {
                StudentFeeId = dto.StudentFeeId,
                AmountPaid = dto.AmountPaid,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                CreatedByUserId = User.GetCurrentUserId().ToString()
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetFeePaymentsByStudentFeeId), new { studentFeeId = dto.StudentFeeId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid payment record request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording fee payment");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error recording fee payment");
        }
    }

    #endregion

    #region Report Endpoints

    /// <summary>
    /// Get fee report with payment status and filters
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="studentId">Filter by student ID</param>
    /// <param name="sectionId">Filter by section ID</param>
    /// <param name="status">Filter by status: Paid, Partial, Due, Overdue</param>
    /// <param name="startDate">Filter by start date (for month selection)</param>
    /// <param name="endDate">Filter by end date (for month selection)</param>
    /// <returns>Paginated fee report with summary statistics</returns>
    [HttpGet("report")]
    [ProducesResponseType(typeof(PaginatedFeeReportListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeeReport(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? studentId = null,
        [FromQuery] string? sectionId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = new GetFeeReportQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                StudentId = studentId,
                SectionId = sectionId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee report");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving fee report");
        }
    }

    #endregion

    #region Fee Receipt PDF Endpoints

    /// <summary>
    /// Generate and download fee receipt PDF
    /// </summary>
    /// <param name="paymentId">Payment ID for which to generate receipt</param>
    /// <returns>PDF file</returns>
    [HttpGet("payments/{paymentId}/receipt")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeeReceiptPdf([FromRoute] string paymentId)
    {
        try
        {
            var command = new GenerateFeeReceiptPdfCommand
            {
                PaymentId = paymentId
            };

            var pdf = await _mediator.Send(command);

            // Return PDF file
            return File(pdf, "application/pdf", $"fee-receipt-{paymentId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Payment not found for receipt generation");
            return NotFound(new { message = "Payment not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating fee receipt PDF");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error generating fee receipt PDF");
        }
    }

    #endregion

}
