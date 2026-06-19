using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Contacts.Commands;

public record DeleteContactCommand(Guid JobApplicationId, Guid ContactId) : IRequest;

public class DeleteContactHandler : IRequestHandler<DeleteContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteContactHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteContactCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.JobApplicationId == request.JobApplicationId, ct)
            ?? throw new KeyNotFoundException($"Contact {request.ContactId} not found");

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync(ct);
    }
}
