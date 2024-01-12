using BL.Models.ViewModels;

namespace TRE_API.Models
{
    public class PolicyInputData
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public int trecount { get; set; }

        public TreInfo tre { get; set; }

        public List<UserExpiryInfo> users { get; set; }

        public class TreData {
            public string name { get; set; }
            public bool active { get; set; }
        }
    }
}
