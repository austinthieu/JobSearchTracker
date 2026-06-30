using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Commands;

public record UploadAttachmentCommand(
    Guid JobApplicationId,
    Stream Content,
    string FileName,
    string ContentType
) : IRequest<Guid>;

public class UploadAttachmentHandler : IRequestHandler<UploadAttachmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadAttachmentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage
    )
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        var jobApp =
            await _context.JobApplications.FirstOrDefaultAsync(
                j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId,
                ct
            )
            ?? throw new KeyNotFoundException(
                $"Job application {request.JobApplicationId} not found"
            );
        using var ms = new MemoryStream();
        await request.Content.CopyToAsync(ms, ct);
        ms.Position = 0;

        var storagePath = await _fileStorage.SaveAsync(
            ms,
            request.FileName,
            request.ContentType,
            ct
        );

        var attachment = new Attachment(
            request.JobApplicationId,
            storagePath,
            request.FileName,
            request.FileName,
            request.ContentType,
            ms.Length
        );

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(ct);
        return attachment.Id;
    }
}
