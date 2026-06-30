using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Queries;

public record GetAttachmentContentQuery(Guid JobApplicationId, Guid AttachmentId)
    : IRequest<AttachmentContentDto?>;

public class AttachmentContentDto
{
    public byte[] Data { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}

public class GetAttachmentContentHandler
    : IRequestHandler<GetAttachmentContentQuery, AttachmentContentDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public GetAttachmentContentHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage
    )
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<AttachmentContentDto?> Handle(
        GetAttachmentContentQuery request,
        CancellationToken ct
    )
    {
        var jobApp = await _context.JobApplications.FirstOrDefaultAsync(
            j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId,
            ct
        );
        if (jobApp is null)
            return null;

        var attachment = await _context.Attachments.FirstOrDefaultAsync(
            a => a.Id == request.AttachmentId && a.JobApplicationId == request.JobApplicationId,
            ct
        );
        if (attachment is null)
            return null;

        var data = await _fileStorage.GetBytesAsync(attachment.StoragePath);

        return new AttachmentContentDto
        {
            Data = data,
            ContentType = attachment.ContentType,
            FileName = attachment.OriginalName,
        };
    }
}
