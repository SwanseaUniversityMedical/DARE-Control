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
            //reads content from Policy file
            policy = File.ReadAllText("poicy.txt");

        }
        public static string GetPolicy()
        {
            return policy;
        }
    }
}
