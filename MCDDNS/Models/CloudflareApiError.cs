using System.Text.Json.Serialization;

namespace MCDDNS.Models;

/// <summary>
/// A single error entry inside a Cloudflare API response.
/// </summary>
public sealed class CloudflareApiError
{
    /// <summary>
    /// Cloudflare's numeric error code.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}