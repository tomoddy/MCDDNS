# MCDDNS

A small dynamic DNS updater for Cloudflare. Keeps a chosen A record synced
to the machine's current public IP — useful for a service behind a
dynamic-IP connection that's reached via direct port-forward rather than a
reverse proxy or tunnel. Self-contained, single-file .NET console app,
meant to be run on a schedule (cron, systemd timer, etc.).

## Usage

```
mc-ddns [--secrets-path=<path>] [--ip-lookup-url=<url>]
```

All arguments are optional. Defaults: `/etc/ansible-secrets/cloudflare-ddns-mc.json`,
`https://api.ipify.org?format=json`.

Exits `0` on success (whether or not an update was needed), `1` on failure
with an error on stderr. No alerting is built in — wire failure
notifications up in whatever runs this on a schedule instead (e.g. via
your scheduler's own failure handling).

## Secrets file (`--secrets-path`)

```json
{
  "cloudflare_api_token": "...",
  "zone_name": "example.com",
  "record_name": "server.example.com"
}
```

Token needs **Zone → DNS → Edit** on the target zone only. Zone/record are
looked up by name each run, not hardcoded IDs.

## Building

```
dotnet publish -c Release
```

Note: `Proxied` is always forced `false` on update — Cloudflare's proxy
doesn't forward raw TCP/UDP.
