using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Timetable.Commands;
using SMS.Application.Features.Timetable.DTOs;
using SMS.Application.Features.Timetable.Queries;
using MediatR;

namespace SMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimetableController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimetableController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // TimeSlot Endpoints
    [HttpGet("timeslots/{academicYearId}")]
    public async Task<ActionResult<List<TimeSlotDto>>> GetTimeSlots(Guid academicYearId)
    {
        return await _mediator.Send(new GetTimeSlotsQuery(academicYearId));
    }

    [HttpPost("timeslots")]
    public async Task<ActionResult<Guid>> CreateTimeSlot(CreateTimeSlotDto timeSlot)
    {
        return await _mediator.Send(new CreateTimeSlotCommand(timeSlot));
    }

    [HttpPut("timeslots/{id}")]
    public async Task<ActionResult<bool>> UpdateTimeSlot(Guid id, CreateTimeSlotDto timeSlot)
    {
        return await _mediator.Send(new UpdateTimeSlotCommand(id, timeSlot));
    }

    [HttpDelete("timeslots/{id}")]
    public async Task<ActionResult<bool>> DeleteTimeSlot(Guid id)
    {
        return await _mediator.Send(new DeleteTimeSlotCommand(id));
    }

    // Timetable Entry Endpoints
    [HttpGet("entries/section/{sectionId}/{academicYearId}")]
    public async Task<ActionResult<List<TimetableEntryDto>>> GetSectionTimetable(Guid sectionId, Guid academicYearId)
    {
        return await _mediator.Send(new GetSectionTimetableQuery(sectionId, academicYearId));
    }

    [HttpGet("entries/teacher/{teacherId}/{academicYearId}")]
    public async Task<ActionResult<List<TimetableEntryDto>>> GetTeacherTimetable(Guid teacherId, Guid academicYearId)
    {
        return await _mediator.Send(new GetTeacherTimetableQuery(teacherId, academicYearId));
    }

    [HttpPost("entries")]
    public async Task<ActionResult<Guid>> CreateEntry(CreateTimetableEntryDto entry)
    {
        try
        {
            return await _mediator.Send(new CreateTimetableEntryCommand(entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("entries/{id}")]
    public async Task<ActionResult<bool>> UpdateEntry(Guid id, CreateTimetableEntryDto entry)
    {
        try
        {
            return await _mediator.Send(new UpdateTimetableEntryCommand(id, entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("entries/{id}")]
    public async Task<ActionResult<bool>> DeleteEntry(Guid id)
    {
        return await _mediator.Send(new DeleteTimetableEntryCommand(id));
    }
}
