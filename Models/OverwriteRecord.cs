using System.Collections.Generic;

namespace DiscordSecurityBot.Models;
public class OverwriteRecord
{
    public string TargetName { get; init; } = string.Empty;
    public string TargetType { get; init; } = string.Empty;
    public List<string> Allow { get; init; } = [];
    public List<string> Deny { get; init; } = [];
}
