using MCDDNS.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MCDDNS;

/// <summary>
/// Thin wrapper over the Cloudflare API v4 endpoints this app needs: look up a zone, look up an A record, and update it.
/// </summary>
public sealed class CloudflareClient
{
    /// <summary>
    /// The underlying HTTP client used to make requests to the Cloudflare API.
    /// </summary>
    private readonly HttpClient HttpClient;

    /// <summary>
    /// Creates a client authenticated with the given Cloudflare API token.
    /// </summary>
    public CloudflareClient(string apiToken)
    {
        HttpClient = new HttpClient { BaseAddress = new Uri("https://api.cloudflare.com/client/v4/") };
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
    }

    /// <summary>
    /// Looks up a zone's ID by its domain name. Throws if the zone doesn't exist or the call fails.
    /// </summary>
    public async Task<string> GetZoneIdAsync(string zoneName)
    {
        CloudflareApiResponse<List<CloudflareZone>>? response = await HttpClient.GetFromJsonAsync<CloudflareApiResponse<List<CloudflareZone>>>($"zones?name={zoneName}");
        if (response is null || !response.Success)
            throw new InvalidOperationException($"Cloudflare zone lookup for '{zoneName}' failed: {DescribeErrors(response)}");
        if (response.Result is null || response.Result.Count == 0)
            throw new InvalidOperationException($"No Cloudflare zone found named '{zoneName}'");
        return response.Result[0].Id;
    }

    /// <summary>
    /// Looks up an A record by name within a zone. Throws if the record doesn't exist or the call fails.
    /// </summary>
    public async Task<CloudflareDnsRecord> GetARecordAsync(string zoneId, string recordName)
    {
        CloudflareApiResponse<List<CloudflareDnsRecord>>? response = await HttpClient.GetFromJsonAsync<CloudflareApiResponse<List<CloudflareDnsRecord>>>($"zones/{zoneId}/dns_records?type=A&name={recordName}");
        if (response is null || !response.Success)
            throw new InvalidOperationException($"Cloudflare DNS record lookup for '{recordName}' failed: {DescribeErrors(response)}");
        if (response.Result is null || response.Result.Count == 0)
            throw new InvalidOperationException($"No A record found named '{recordName}'");
        return response.Result[0];
    }

    /// <summary>
    /// Updates an existing A record's IP address, keeping its TTL and forcing proxied to false (raw Minecraft TCP can't go through Cloudflare's proxy).
    /// </summary>
    public async Task UpdateRecordIpAsync(string zoneId, CloudflareDnsRecord record, string newIp)
    {
        CloudflareDnsRecord body = new()
        {
            Id = record.Id,
            Type = "A",
            Name = record.Name,
            Content = newIp,
            Ttl = record.Ttl,
            Proxied = false
        };
        HttpResponseMessage response = await HttpClient.PatchAsJsonAsync($"zones/{zoneId}/dns_records/{record.Id}", body);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Cloudflare DNS record update failed: HTTP {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }

    /// <summary>
    /// Describes the errors in a Cloudflare API response, or indicates that there was no response body.
    /// </summary>
    /// <typeparam name="T">The type of the result in the Cloudflare API response.</typeparam>
    /// <param name="response">The Cloudflare API response to describe errors for.</param>
    /// <returns>A string describing the errors, or indicating that there was no response body.</returns>
    private static string DescribeErrors<T>(CloudflareApiResponse<T>? response) => response is null ? "no response body" : string.Join("; ", response.Errors.Select(e => $"{e.Code}: {e.Message}"));
}