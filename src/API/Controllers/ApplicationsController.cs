using Application.JobApplications.Commands;
using Application.JobApplications.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly ISender _mediator;

    public ApplicationsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var applications = await _mediator.Send(new GetJobApplicationsQuery());
        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _mediator.Send(new GetJobApplicationByIdQuery(id));
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJobApplicationCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusRequest request)
    {
        if (!Enum.TryParse<ApplicationStatus>(request.Status, true, out var status))
            return BadRequest(
                new
                {
                    error = $"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<ApplicationStatus>())}",
                }
            );

        await _mediator.Send(new UpdateStatusCommand(id, status));
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(Guid id, AddNoteRequest request)
    {
        var noteId = await _mediator.Send(new AddNoteCommand(id, request.Content));
        return CreatedAtAction(nameof(GetById), new { id }, new { noteId });
    }
}

// Request DTOs
public class UpdateStatusRequest
{
    public string Status { get; set; } = null!;
}

public class AddNoteRequest
{
    public string Content { get; set; } = null!;
}
