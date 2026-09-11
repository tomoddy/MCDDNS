using MCDDNS.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MCDDNS;

/// <summary>
/// Entry point: keeps a Cloudflare A record synced to the current public IP, so mc-direct.tzer0m.co.uk (and via its SRV record, mc.tzer0m.co.uk's Minecraft game port) keeps resolving to the right home WAN address.
/// </summary>
public static class Program
{
    /// <summary>
    /// Default value for --secrets-path, used when that argument isn't given.
    /// </summary>
    private const string DefaultSecretsPath = "/etc/ansible-secrets/cloudflare-ddns-mc.json";

    /// <summary>
    /// Default value for --ip-lookup-url, used when that argument isn't given.
    /// </summary>
    private const string DefaultIpLookupUrl = "https://api.ipify.org?format=json";

    /// <summary>
    /// Runs the DDNS check/update. Accepts optional --secrets-path= and --ip-lookup-url= arguments, each falling back to its default if not given. Returns 0 on success (whether or not an update was needed), 1 on any failure. Failure alerting is handled by Semaphore itself, not by this app.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        // Parse command-line arguments, falling back to defaults if not given.
        string secretsPath = GetArg(args, "--secrets-path", DefaultSecretsPath);
        string ipLookupUrl = GetArg(args, "--ip-lookup-url", DefaultIpLookupUrl);

        try
        {
            // Load the secrets file, which contains the Cloudflare API token and the DNS record details.
            if (!File.Exists(secretsPath))
            {
                Console.Error.WriteLine($"ERROR: secrets file not found at {secretsPath}");
                return 1;
            }

            // Deserialize the secrets JSON into a Secrets object, and validate that all required fields are present.
            await using FileStream secretsStream = File.OpenRead(secretsPath);
            Secrets? secrets = await JsonSerializer.DeserializeAsync<Secrets>(secretsStream);
            if (secrets is null || string.IsNullOrEmpty(secrets.CloudflareApiToken) || string.IsNullOrEmpty(secrets.ZoneName) || string.IsNullOrEmpty(secrets.RecordName))
            {
                Console.Error.WriteLine($"ERROR: {secretsPath} is missing required fields");
                return 1;
            }

            // Fetch the current public IP address of the machine running this code, using the specified IP lookup URL.
            string currentIp = await GetPublicIpAsync(ipLookupUrl);

            // Initialize the Cloudflare client with the API token, get the zone ID for the specified zone name, and retrieve the current A record for the specified record name.
            CloudflareClient cloudflare = new(secrets.CloudflareApiToken);
            string zoneId = await cloudflare.GetZoneIdAsync(secrets.ZoneName);
            CloudflareDnsRecord record = await cloudflare.GetARecordAsync(zoneId, secrets.RecordName);

            // If the current A record already points to the current public IP, log that no update is needed and exit successfully.
            if (record.Content == currentIp)
            {
                Console.WriteLine($"OK: {secrets.RecordName} already points at {currentIp}, no update needed");
                return 0;
            }

            // Otherwise, update the A record to point to the current public IP and log the change.
            string oldIp = record.Content;
            await cloudflare.UpdateRecordIpAsync(zoneId, record, currentIp);

            Console.WriteLine($"CHANGED: {secrets.RecordName} updated {oldIp} -> {currentIp}");
            return 0;
        }
        catch (Exception ex)
        {
            // Log the error to the console. Semaphore's own failure alerting picks this up via the Ansible task's failed_when.
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Fetches the caller's current public IPv4 address from the given lookup URL, which must return JSON shaped like { "ip": "..." } (api.ipify.org's format).
    /// </summary>
    private static async Task<string> GetPublicIpAsync(string ipLookupUrl)
    {
        using HttpClient http = new();
        JsonDocument document = await http.GetFromJsonAsync<JsonDocument>(ipLookupUrl) ?? throw new InvalidOperationException("Empty response from IP lookup service");
        return document.RootElement.GetProperty("ip").GetString() ?? throw new InvalidOperationException("IP lookup response had no 'ip' field");
    }

    /// <summary>
    /// Looks for a "--name=value" argument in args and returns its value, or defaultValue if not present.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <param name="name">The argument name, including its leading "--".</param>
    /// <param name="defaultValue">The value to return if the argument isn't present.</param>
    /// <returns>The argument's value, or defaultValue.</returns>
    private static string GetArg(string[] args, string name, string defaultValue)
    {
        string prefix = $"{name}=";
        foreach (string arg in args)
        {
            if (arg.StartsWith(prefix, StringComparison.Ordinal))
                return arg[prefix.Length..];
        }
        return defaultValue;
    }
}
