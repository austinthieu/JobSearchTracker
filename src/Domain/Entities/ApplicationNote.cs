using Domain.Common;

namespace Domain.Entities;

public class ApplicationNote : Entity
{
    public string Content { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // Foreign Keys
    public Guid JobApplicationId { get; private set; }

    // Navigation
    public JobApplication JobApplication { get; private set; } = null!;

    private ApplicationNote() { }

    public ApplicationNote(Guid jobApplicationId, string content)
    {
        JobApplicationId = jobApplicationId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }
}

