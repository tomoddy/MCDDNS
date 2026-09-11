using System.Text.Json.Serialization;

namespace MCDDNS.Models;

/// <summary>
/// A DNS record as returned by/sent to GET and PATCH /zones/{zoneId}/dns_records.
/// </summary>
public sealed class CloudflareDnsRecord
{
    /// <summary>
    /// The record's Cloudflare-internal ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The DNS record type, e.g. "A".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "A";

    /// <summary>
    /// The record's hostname, e.g. "mc-direct.tzer0m.co.uk".
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The record's value — for an A record, the IPv4 address.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Time-to-live in seconds.
    /// </summary>
    [JsonPropertyName("ttl")]
    public int Ttl { get; set; } = 300;

    /// <summary>
    /// Whether the record is proxied through Cloudflare — must stay false here, since raw Minecraft TCP can't go through the proxy.
    /// </summary>
    [JsonPropertyName("proxied")]
    public bool Proxied { get; set; } = false;
}