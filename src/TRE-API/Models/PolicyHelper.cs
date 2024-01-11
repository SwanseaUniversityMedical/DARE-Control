namespace TRE_API.Models
{
    public class PolicyHelper
    {
        private static string policy;
        static PolicyHelper()
        {
            LoadPolicyFromFile();
        }

        private static void LoadPolicyFromFile()
        {
        
            policy = File.ReadAllText("policy.txt");

        }
        public static string GetPolicy()
        {
            return policy;
        }
    }
}
