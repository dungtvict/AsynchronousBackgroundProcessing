namespace AsynchronousBackgroundProcessing.Configurations
{
    public class MyJobConfiguration
    {
        public string? BackgroundJobId { get; set; }

        public string? JobCode { get; set; }

        public string? JobName { get; set; }

        public string? CronExpression { get; set; }

        public bool Enabled { get; set; }
    }
}