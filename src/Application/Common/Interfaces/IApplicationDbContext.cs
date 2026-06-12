using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
  DbSet<User> Users { get; }
  DbSet<Company> Companies { get; }
  DbSet<JobApplication> JobApplications { get; }
  DbSet<Interview> Interviews { get; }
  DbSet<Contact> Contacts { get; }
  DbSet<Attachment> Attachments { get; }
  DbSet<ApplicationNote> ApplicationNotes { get; }
  DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


