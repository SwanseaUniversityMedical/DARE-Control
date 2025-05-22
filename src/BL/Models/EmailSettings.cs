namespace BL.Models
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }

        public HashSet<string> EmailsToIgnore { get; set; }

        public string EmailOverride { get; set; }

        public bool Enabled { get; set; }
    }
}