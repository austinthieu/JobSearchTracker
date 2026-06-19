using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Commands;

public record UploadAttachmentCommand(
    Guid JobApplicationId,
    string FileName,
    string OriginalName,
    string ContentType,
    long SizeBytes
) : IRequest<Guid>;

public class UploadAttachmentHandler : IRequestHandler<UploadAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UploadAttachmentHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        var attachment = new Attachment(
            request.JobApplicationId, request.FileName, request.OriginalName,
            request.ContentType, request.SizeBytes
        );

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(ct);
        return attachment.Id;
    }
}
