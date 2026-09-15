namespace Krono
{
    /// <summary>
    /// Config for cronjob - deserialized from Yaml.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// Name of cronjob. Must be filesystem safe, will fail startup check if not. Required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User job will be run as. If not set run as the user
        /// that started Krono. Optional.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Cronmask to control timer for executing this job.  Required.
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// Shell command to execute. Required.
        /// </summary>
        public string Command  { get; set; }

        /// <summary>
        /// Set to false to stop job from loading. Requires process restart. Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// If true, an email alert is sent whenever job runs, regardless of outcome. 
        /// Default is false.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Email address to send alerts to. If not set defaults to Settings.ReceiverAddress value.
        /// If empty, no alerts will be sent. If this and Settings.ReceiverAddress set, this will
        /// take precedence. Optional.
        /// </summary>
        public string ReceiverAddress { get; set; }

        public override string ToString()
        {
            return 
                $"Command : {this.Command}\n" +
                $"Enabled : {this.Enabled}\n" +
                $"Mask : {this.Mask}\n" +
                $"Name : {this.Name}\n" +
                $"ReceiverAddress : {this.ReceiverAddress}\n" +
                $"User : {this.User}\n" +
                $"Verbose : {this.Verbose}\n";
        }
    }
}