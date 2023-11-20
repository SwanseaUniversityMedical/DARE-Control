﻿namespace TRE_API.Models
{
    public class AgentSettings
    {
        public bool UseRabbit { get; set; }
        public bool UseHutch { get; set; }
        public bool UseTESK { get; set; }

        public string TESKOutputBucketPrefix { get; set; }

        public string ImageNameToAddToToken { get; set; }

        public bool Proxy { get; set; }

        public string ProxyAddresURL { get; set; }

        public string BypassProxy { get; set; }

        public string TESKAPIURL { get; set; }
        public string URLHasuraToAdd { get; set; }
    }
}
