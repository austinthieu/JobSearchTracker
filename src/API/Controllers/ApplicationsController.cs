using Application.Attachments.Commands;
using Application.Attachments.Queries;
using Application.Contacts.Commands;
using Application.Contacts.Queries;
using Application.Interviews.Commands;
using Application.Interviews.Queries;
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
    public async Task<IActionResult> GetAll([FromQuery] int Page = 1, [FromQuery] int PageSize = 25)
    {
        var applications = await _mediator.Send(new GetJobApplicationsQuery(Page, PageSize));
        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _mediator.Send(new GetJobApplicationByIdQuery(id));
        return application is null ? NotFound() : Ok(application);
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var application = await _mediator.Send(new GetJobApplicationDetailQuery(id));
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

    // ── Interviews ──────────────────────────────────────────

    [HttpGet("{id:guid}/interviews")]
    public async Task<IActionResult> GetInterviews(Guid id)
    {
        var interviews = await _mediator.Send(new GetInterviewsQuery(id));
        return Ok(interviews);
    }

    [HttpPost("{id:guid}/interviews")]
    public async Task<IActionResult> CreateInterview(Guid id, CreateInterviewRequest request)
    {
        if (!Enum.TryParse<InterviewType>(request.Type, true, out _))
            return BadRequest(
                new
                {
                    error = $"Invalid interview type. Valid values: {string.Join(", ", Enum.GetNames<InterviewType>())}",
                }
            );

        var command = new CreateInterviewCommand(
            id,
            request.Type,
            request.ScheduledAt,
            request.DurationMinutes,
            request.Location,
            request.Interviewers,
            request.Notes
        );
        var interviewId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { interviewId });
    }

    [HttpPatch("{id:guid}/interviews/{interviewId:guid}/reschedule")]
    public async Task<IActionResult> RescheduleInterview(
        Guid id,
        Guid interviewId,
        RescheduleRequest request
    )
    {
        await _mediator.Send(
            new RescheduleInterviewCommand(id, interviewId, request.NewScheduledAt)
        );
        return NoContent();
    }

    [HttpPost("{id:guid}/interviews/{interviewId:guid}/follow-up")]
    public async Task<IActionResult> MarkFollowUpSent(Guid id, Guid interviewId)
    {
        await _mediator.Send(new MarkFollowUpSentCommand(id, interviewId));
        return NoContent();
    }

    // ── Contacts ────────────────────────────────────────────

    [HttpGet("{id:guid}/contacts")]
    public async Task<IActionResult> GetContacts(Guid id)
    {
        var contacts = await _mediator.Send(new GetContactsQuery(id));
        return Ok(contacts);
    }

    [HttpPost("{id:guid}/contacts")]
    public async Task<IActionResult> CreateContact(Guid id, CreateContactRequest request)
    {
        var command = new CreateContactCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Title,
            request.Notes
        );
        var contactId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { contactId });
    }

    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    public async Task<IActionResult> DeleteContact(Guid id, Guid contactId)
    {
        await _mediator.Send(new DeleteContactCommand(id, contactId));
        return NoContent();
    }

    // ── Attachments ─────────────────────────────────────────

    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> GetAttachments(Guid id)
    {
        var attachments = await _mediator.Send(new GetAttachmentsQuery(id));
        return Ok(attachments);
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> UploadAttachment(Guid id, UploadAttachmentRequest request)
    {
        var command = new UploadAttachmentCommand(
            id,
            request.FileName,
            request.OriginalName,
            request.ContentType,
            request.SizeBytes
        );
        var attachmentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { attachmentId });
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
    {
        await _mediator.Send(new DeleteAttachmentCommand(id, attachmentId));
        return NoContent();
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

public class CreateInterviewRequest
{
    public string Type { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? Interviewers { get; set; }
    public string? Notes { get; set; }
}

public class RescheduleRequest
{
    public DateTime NewScheduledAt { get; set; }
}

public class CreateContactRequest
{
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
}

public class UploadAttachmentRequest
{
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
}
