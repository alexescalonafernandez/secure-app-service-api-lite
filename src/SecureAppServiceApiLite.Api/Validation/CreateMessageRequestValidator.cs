using FluentValidation;
using SecureAppServiceApiLite.Api.Contracts;

namespace SecureAppServiceApiLite.Api.Validation;

public sealed class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    private static readonly string[] AllowedPriorities = ["Low", "Normal", "High"];

    public CreateMessageRequestValidator()
    {
        RuleFor(request => request.Subject)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Body)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(request => request.Priority)
            .Must(priority =>
                string.IsNullOrWhiteSpace(priority) ||
                AllowedPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Priority must be Low, Normal, or High.");
    }
}
