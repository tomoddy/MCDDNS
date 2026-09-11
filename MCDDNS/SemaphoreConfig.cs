using System.Text.Json.Serialization;

namespace MCDDNS;

/// <summary>
/// The subset of /etc/semaphore/config.json this app reads, to reuse Semaphore's existing Telegram bot rather than requiring separate credentials.
/// </summary>
public sealed class SemaphoreConfig
{
    /// <summary>
    /// Telegram bot token, already used by Semaphore's own task-failure alerts.
    /// </summary>
    [JsonPropertyName("telegram_token")]
    public string? TelegramToken { get; set; }

    /// <summary>
    /// Telegram chat ID Semaphore's alerts are sent to.
    /// </summary>
    [JsonPropertyName("telegram_chat")]
    public string? TelegramChat { get; set; }
}