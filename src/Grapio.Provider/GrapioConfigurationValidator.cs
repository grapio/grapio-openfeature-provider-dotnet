using FluentValidation;

namespace Grapio.Provider;

public class GrapioConfigurationValidator: AbstractValidator<GrapioConfiguration>
{
    public GrapioConfigurationValidator()
    {
        RuleFor(c => c.ConnectionString)
            .NotEmpty()
            .Must(v => !v.Contains(":memory:"))
            .WithMessage("Connection string is invalid or contains :memory:");

        RuleFor(c => c.Requester)
            .NotEmpty()
            .When(c => !c.Offline)
            .WithMessage("A requester must be specified when Offline is false.");
        
        RuleFor(c => c.RefreshInterval)
            .GreaterThanOrEqualTo(5)
            .LessThanOrEqualTo(7200)
            .When(c => !c.Offline)
            .WithMessage("Refresh interval must be >= 5 and <= 7200 when Offline is false.");
        
        RuleFor(c => c.ServerUri)
            .NotEmpty()
            .Must(v => Uri.IsWellFormedUriString(v, UriKind.Absolute))
            .When(c => !c.Offline)
            .WithMessage("Server URI is invalid and Offline is false.");
    }
}