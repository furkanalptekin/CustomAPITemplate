using FluentValidation;

namespace CustomAPITemplate.Contract.V1.Validators;
public class UserRoleRequestValidator : AbstractValidator<UserRoleRequest>
{
    public UserRoleRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .NotEmpty();
    }
}