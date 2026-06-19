using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Contacts.Queries;

public record GetContactsQuery(Guid JobApplicationId) : IRequest<List<ContactDto>>;

public class GetContactsHandler : IRequestHandler<GetContactsQuery, List<ContactDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetContactsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ContactDto>> Handle(GetContactsQuery request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        return await _context.Contacts
            .Where(c => c.JobApplicationId == request.JobApplicationId)
            .OrderBy(c => c.Name)
            .Select(c => new ContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Title = c.Title,
                Notes = c.Notes,
            })
            .ToListAsync(ct);
    }
}
