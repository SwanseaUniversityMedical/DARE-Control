namespace Tre_Camunda.Settings
{
    public class MinioSettings
    {
        public string Url { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Alias { get; set; } = "dareminio";
        public string AWSRegion { get; set; } = "us-east-1";
        public string AWSService { get; set; } = "s3";
        public string AttributeName { get; set; } = "policy";
        public string AdminConsole { get; set; } = string.Empty;
        public string ProxyAddresURLForExternalFetch { get; set; } = string.Empty;
    }
}
