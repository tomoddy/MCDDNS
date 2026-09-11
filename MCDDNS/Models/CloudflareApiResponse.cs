using System.Text.Json.Serialization;

namespace MCDDNS.Models;

/// <summary>
/// Generic envelope every Cloudflare API v4 response is wrapped in.
/// </summary>
public sealed class CloudflareApiResponse<T>
{
    /// <summary>
    /// Whether the API call succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Error entries returned when Success is false.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<CloudflareApiError> Errors { get; set; } = [];

    /// <summary>
    /// The actual payload, shaped differently per endpoint (a single object or a list).
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; set; }
}