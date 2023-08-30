
namespace BL.Models.ViewModels
{
    public class KeycloakUser
    {
        public string id { get; set; }
        public long createdTimestamp { get; set; }
        public string username { get; set; }
        public bool enabled { get; set; }
        public bool totp { get; set; }
        public bool emailVerified { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public UserAttributes attributes { get; set; }
        public string[] disableableCredentialTypes { get; set; }
        public string[] requiredActions { get; set; }
        public long notBefore { get; set; }
        public Access access { get; set; }
    }

    public class UserAttributes
    {
        public string[] policy { get; set; }
    }

    public class Access
    {
        public bool manageGroupMembership { get; set; }
        public bool view { get; set; }
        public bool mapRoles { get; set; }
        public bool impersonate { get; set; }
        public bool manage { get; set; }
    }
}
