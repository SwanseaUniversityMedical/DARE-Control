
namespace BL.Models.ViewModels
{
    public class MinioSettings
    {
        public string Url { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string AWSRegion { get; set; }
        public string AWSService { get; set; }
        public string AttributeName { get; set; }
    }
}
