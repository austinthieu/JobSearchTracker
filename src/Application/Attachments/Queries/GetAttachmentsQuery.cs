using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Attachments.Queries;

public class AttachmentItemDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}

public record GetAttachmentsQuery(Guid JobApplicationId) : IRequest<List<AttachmentItemDto>>;

public class GetAttachmentsHandler : IRequestHandler<GetAttachmentsQuery, List<AttachmentItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAttachmentsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AttachmentItemDto>> Handle(GetAttachmentsQuery request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        return await _context.Attachments
            .Where(a => a.JobApplicationId == request.JobApplicationId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentItemDto
            {
                Id = a.Id,
                FileName = a.FileName,
                OriginalName = a.OriginalName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes,
                UploadedAt = a.UploadedAt,
            })
            .ToListAsync(ct);
    }
}
