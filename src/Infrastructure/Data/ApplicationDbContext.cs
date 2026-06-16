using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.NormalizedEmail).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();

                modelBuilder.Entity<Company>(entity =>
                {
                      entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
                      entity.HasOne(c => c.User)
                .WithMany(u => u.Companies)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                  });

                modelBuilder.Entity<JobApplication>(entity =>
            {
                  entity.Property(j => j.Position).HasMaxLength(200).IsRequired();
                  entity.HasOne(j => j.User)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                  entity.HasOne(j => j.Company)
                .WithMany(c => c.JobApplications)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
              });

                modelBuilder.Entity<Interview>(entity =>
            {
                  entity.HasOne(i => i.JobApplication)
                .WithMany(j => j.Interviews)
                .HasForeignKey(i => i.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
              });

                modelBuilder.Entity<Contact>(entity =>
            {
                  entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
                  entity.HasOne(c => c.JobApplication)
                .WithMany(j => j.Contacts)
                .HasForeignKey(c => c.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
              });

                modelBuilder.Entity<Attachment>(entity =>
            {
                  entity.HasOne(a => a.JobApplication)
                .WithMany(j => j.Attachments)
                .HasForeignKey(a => a.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
              });

                modelBuilder.Entity<ApplicationNote>(entity =>
            {
                  entity.Property(n => n.Content).IsRequired();
                  entity.HasOne(n => n.JobApplication)
                .WithMany(j => j.Notes)
                .HasForeignKey(n => n.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
              });

                modelBuilder.Entity<ApplicationStatusHistory>(entity =>
            {
                  entity.HasOne(h => h.JobApplication)
                .WithMany(j => j.StatusHistory)
                .HasForeignKey(h => h.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
              });
            });
    }
}
