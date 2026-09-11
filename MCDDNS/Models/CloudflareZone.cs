using System.Text.Json.Serialization;

namespace MCDDNS.Models;

/// <summary>
/// A Cloudflare zone (roughly: a domain) as returned by GET /zones.
/// </summary>
public sealed class CloudflareZone
{
    /// <summary>
    /// The zone's Cloudflare-internal ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The zone's domain name, e.g. "tzer0m.co.uk".
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}