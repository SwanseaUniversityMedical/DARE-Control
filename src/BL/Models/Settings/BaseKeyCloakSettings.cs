using System.Net;
using Serilog;

namespace BL.Models.Settings;

public class BaseKeyCloakSettings
{
    public string Authority { get; set; } = string.Empty;
    public string RootUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RemoteSignOutPath { get; set; } = string.Empty;
    public string SignedOutRedirectUri { get; set; } = string.Empty;
    public string TokenExpiredAddress { get; set; } = string.Empty;
    public bool Proxy { get; set; }

    public string BypassProxy { get; set; } = string.Empty;

    public string ProxyAddressUrl { get; set; } = string.Empty;
    public string TokenRefreshSeconds { get; set; } = string.Empty;

    public bool KeycloakDemoMode { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;

    public bool UseRedirectUrl { get; set; }

    public string MetadataAddress { get; set; } = string.Empty;
    public string? ValidAudiences { get; set; }
    public string Server { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public bool AutoTrustKeycloakCert { get; set; }
    public string ValidIssuer { get; set; } = string.Empty;

    public string ValidAudience { get; set; } = string.Empty;

    public HttpClientHandler GetProxyHandler
    {
        get
        {
            Log.Information($"getProxyHandler ProxyAddressURL > {ProxyAddressUrl} Proxy > {Proxy} ");
            HttpClientHandler handler = new HttpClientHandler
            {
                Proxy = string.IsNullOrWhiteSpace(ProxyAddressUrl)
                    ? null
                    : new WebProxy(ProxyAddressUrl, true), // Replace with your proxy server URL
                UseProxy = Proxy
            };
            return handler;
        }
    }
}