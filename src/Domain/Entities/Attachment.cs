using Domain.Common;

namespace Domain.Entities;

public class Attachment : Entity
{
    public string FileName { get; private set; } = null!;
    public string OriginalName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string StoragePath { get; private set; } = null!;
    public DateTime UploadedAt { get; private set; }

    // Foreign Keys
    public Guid JobApplicationId { get; private set; }

    // Navigation
    public JobApplication JobApplication { get; private set; } = null!;

    private Attachment() { }

    public Attachment(
        Guid jobApplicationId,
        string storagePath,
        string fileName,
        string originalName,
        string contentType,
        long sizeBytes
    )
    {
        JobApplicationId = jobApplicationId;
        StoragePath = storagePath;
        FileName = fileName;
        OriginalName = originalName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedAt = DateTime.UtcNow;
    }
}
