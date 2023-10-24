namespace TRE_API.Models
{
    public class AgentSettings
    {
        public bool UseRabbit { get; set; }
        public bool UseHutch { get; set; }
        public bool UseTESK { get; set; }

        public string TESKOutputBucketPrefix { get; set; }

    }
}
