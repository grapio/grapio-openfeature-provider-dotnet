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

        RuleFor(c => c.ZmqProxyAddress)
            .Matches(@"^(tcp|udp)://[\d]{1,3}.[\d]{1,3}.[\d]{1,3}.[\d]{1,3}:[\d]+$")
            .When(c => !string.IsNullOrEmpty(c.ZmqProxyAddress))
            .WithMessage("ZmqAddress must match the format <protocol>://<ip address>:<port>");
    }    
}
