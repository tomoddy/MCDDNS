using System.Text.Json.Serialization;

namespace MCDDNS;

/// <summary>
/// Credentials and configuration loaded from /etc/ansible-secrets/cloudflare-ddns-mc.json on Tyrion.
/// </summary>
public sealed class Secrets
{
    /// <summary>
    /// Cloudflare API token, scoped to Zone:DNS:Edit on the target zone only.
    /// </summary>
    [JsonPropertyName("cloudflare_api_token")]
    public string CloudflareApiToken { get; set; } = string.Empty;

    /// <summary>
    /// The Cloudflare zone name, e.g. "tzer0m.co.uk".
    /// </summary>
    [JsonPropertyName("zone_name")]
    public string ZoneName { get; set; } = string.Empty;

    /// <summary>
    /// The A record to keep in sync, e.g. "mc-direct.tzer0m.co.uk".
    /// </summary>
    [JsonPropertyName("record_name")]
    public string RecordName { get; set; } = string.Empty;
}