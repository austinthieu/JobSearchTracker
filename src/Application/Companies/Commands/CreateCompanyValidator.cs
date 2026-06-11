using FluentValidation;

namespace Application.Companies.Commands;

public class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
  public CreateCompanyValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
  }
}
