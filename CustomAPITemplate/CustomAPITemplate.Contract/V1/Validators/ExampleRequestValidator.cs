using FluentValidation;

namespace CustomAPITemplate.Contract.V1.Validators;

public class ExampleRequestValidator : AbstractValidator<ExampleRequest>
{
    public ExampleRequestValidator()
    {
        RuleFor(x => x.Test1)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(100);

        RuleFor(x => x.Test2)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(100);
    }
}