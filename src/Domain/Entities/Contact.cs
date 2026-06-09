using Domain.Common;

namespace Domain.Entities;

public class Contact : Entity
{
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Title { get; private set; }
    public string? Notes { get; private set; }

    // Foreign keys
    public Guid JobApplicationId { get; private set; }

    // Navigation
    public JobApplication JobApplication { get; private set; } = null!;

    private Contact() { }

    public Contact(Guid jobApplicationId, string name, string? email, string? phone, string? title, string? notes)
    {
        JobApplicationId = jobApplicationId;
        Name = name;
        Email = email;
        Phone = phone;
        Title = title;
        Notes = notes;
    }
}
