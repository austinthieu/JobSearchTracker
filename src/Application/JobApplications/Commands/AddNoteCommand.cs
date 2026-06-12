using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.JobApplications.Commands;

public record AddNoteCommand(Guid JobApplicationId, string Content) : IRequest<Guid>;

public class AddNoteHandler : IRequestHandler<AddNoteCommand, Guid>
{
  private readonly IApplicationDbContext _context;

  public AddNoteHandler(IApplicationDbContext context) => _context = context;

  public async Task<Guid> Handle(AddNoteCommand request, CancellationToken ct)
  {
    var applicationNote = new ApplicationNote(request.JobApplicationId, request.Content);
    _context.ApplicationNotes.Add(applicationNote);
    await _context.SaveChangesAsync(ct);
    return applicationNote.Id;
  }
}
