namespace BL.Models.Settings
{
    public class BaseKeyCloakSettings
    {
        public string Authority { get; set; }

        public string BaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RemoteSignOutPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string TokenExpiredAddress { get; set; }
        public bool Proxy { get; set; }

        public string BypassProxy { get; set; }

        public string ProxyAddresURL { get; set; }
        public string TokenRefreshSeconds { get; set; }
        

        public string RedirectURL { get; set; }

        public bool UseRedirectURL { get; set; }

        public string MetadataAddress { get; set; }
        public string? ValidAudiences { get; set; }
    }
}
