using System.Net.Http.Json;
using System.Text.Json;

namespace MCDDNS;

/// <summary>
/// Sends best-effort alert messages to the Telegram bot/chat already configured for Semaphore's own task-failure alerts, read from a Semaphore config.json.
/// </summary>
/// <param name="semaphoreConfigPath">Path to Semaphore's config.json.</param>
public sealed class TelegramNotifier(string semaphoreConfigPath)
{
    /// <summary>
    /// The path to Semaphore's config.json, which contains the Telegram bot token and chat ID.
    /// </summary>
    private readonly string SemaphoreConfigPath = semaphoreConfigPath;

    /// <summary>
    /// The HTTP client used to send messages to the Telegram API.
    /// </summary>
    private readonly HttpClient HttpClient = new();

    /// <summary>
    /// Sends a message via Telegram. A failed or unconfigured Telegram send should not fail the DDNS run itself.
    /// </summary>
    public async Task SendAsync(string message)
    {
        try
        {
            // Read Semaphore's config.json to get the Telegram bot token and chat ID. If the file doesn't exist or is malformed, we just skip sending the message.
            if (!File.Exists(SemaphoreConfigPath))
                return;

            // Read the config.json file and deserialize it into a SemaphoreConfig object.
            await using FileStream stream = File.OpenRead(SemaphoreConfigPath);
            SemaphoreConfig? config = await JsonSerializer.DeserializeAsync<SemaphoreConfig>(stream);
            if (config is null || string.IsNullOrEmpty(config.TelegramToken) || string.IsNullOrEmpty(config.TelegramChat))
                return;

            // Send the message to the Telegram bot using the bot token and chat ID from the config.
            string url = $"https://api.telegram.org/bot{config.TelegramToken}/sendMessage";
            await HttpClient.PostAsJsonAsync(url, new { chat_id = config.TelegramChat, text = message });
        }
        catch { }
    }
}