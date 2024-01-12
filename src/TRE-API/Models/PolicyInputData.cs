using BL.Models.ViewModels;

namespace TRE_API.Models
{
    public class PolicyInputData
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public int TreCount { get; set; }

        public TreInfo Tre { get; set; }

        public List<UserExpiryInfo> Users { get; set; }

        public class TreInfo {
            public string Name { get; set; }
            public bool Active { get; set; }
        }
    }
}
