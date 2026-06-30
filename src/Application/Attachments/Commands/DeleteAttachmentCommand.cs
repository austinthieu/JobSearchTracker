using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Commands;

public record DeleteAttachmentCommand(Guid JobApplicationId, Guid AttachmentId) : IRequest;

public class DeleteAttachmentHandler : IRequestHandler<DeleteAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public DeleteAttachmentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage
    )
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task Handle(DeleteAttachmentCommand request, CancellationToken ct)
    {
        var jobApp =
            await _context.JobApplications.FirstOrDefaultAsync(
                j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId,
                ct
            )
            ?? throw new KeyNotFoundException(
                $"Job application {request.JobApplicationId} not found"
            );

        var attachment =
            await _context.Attachments.FirstOrDefaultAsync(
                a => a.Id == request.AttachmentId && a.JobApplicationId == request.JobApplicationId,
                ct
            ) ?? throw new KeyNotFoundException($"Attachment {request.AttachmentId} not found");

        await _fileStorage.DeleteAsync(attachment.StoragePath);
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync(ct);
    }
}
