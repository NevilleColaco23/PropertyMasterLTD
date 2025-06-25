using MyWarehouse.Infrastructure.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MyWarehouse.Infrastructure.Logging.Settings;

public class LogglySettings
{
    [Required]
    [MemberNotNullWhen(true, nameof(CustomerToken))]
    public bool? WriteToLoggly { get; init; }

    [RequiredIf(nameof(WriteToLoggly), true)]
    public string? CustomerToken { get; init; }
}
