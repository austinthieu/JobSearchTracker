using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Contacts.Commands;

public record CreateContactCommand(
    Guid JobApplicationId,
    string Name,
    string? Email,
    string? Phone,
    string? Title,
    string? Notes
) : IRequest<Guid>;

public class CreateContactHandler : IRequestHandler<CreateContactCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateContactHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateContactCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        var contact = new Contact(
            request.JobApplicationId, request.Name, request.Email,
            request.Phone, request.Title, request.Notes
        );

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync(ct);
        return contact.Id;
    }
}
