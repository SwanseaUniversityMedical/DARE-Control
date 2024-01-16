using BL.Models;

namespace TRE_API.Models
{
    public class PolicyInputData
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public int trecount { get; set; }

        public TreClass tre { get; set; }

    }
        public class TreClass {
            public string name { get; set; }
            public bool active { get; set; }
        public List<UserExpiryInfo> users { get; set; }
    }
    }

