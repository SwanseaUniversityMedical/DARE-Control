namespace BL.Models.Settings
{
    public class KeyCloakSettings
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RemoteSignOutPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string TokenExpiredAddress { get; set; }
        public string? ValidAudiences { get; set; }
    }
}
