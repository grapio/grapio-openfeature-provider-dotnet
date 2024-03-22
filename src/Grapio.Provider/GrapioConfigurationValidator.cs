using FluentValidation;

namespace Grapio.Provider;

/// <summary>
/// Validates the properties of the <see cref="GrapioConfiguration"/>.
/// </summary>
internal class GrapioConfigurationValidator : AbstractValidator<GrapioConfiguration>
{
    /// <inheritdoc />
    public GrapioConfigurationValidator()
    {
        RuleFor(c => c.ConnectionString)
            .NotEmpty()
            .WithMessage("A valid LiteDb connection string must be set.");

        RuleFor(c => c.ServerAddress)
            .NotEmpty()
            .WithMessage("A valid Grapio Server address is required when LoadFeatureFlagsFromServer is set to true.")
            .When(c => c.LoadFeatureFlagsFromServer);
        
        RuleFor(c => c.RefreshInterval)
            .InclusiveBetween(5, 7200)
            .WithMessage("Refresh interval must be between 5 and 7200 seconds (inclusive).");
    }
}
