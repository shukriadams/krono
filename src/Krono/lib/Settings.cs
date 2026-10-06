namespace Krono
{

    public class Settings : SettingsBase
    {
        #region PROPERTIES

        public IEnumerable<Job> Jobs { get; set; } = new Job [] {};

        /// <summary>
        /// Set to false to disable all jobs.
        /// </summary>
        public bool Enabled { get; set; } = true;

        public string ReceiverAddress { get; set; }
        
        public bool EmailNotifications { get; set; } 

        public string SenderAddress { get; set; }

        public string LogRoot { get; set; } = "/var/log/krono";

        public override string ToString()
        {
            return $"Enabled:{this.Enabled}\n" +
                $"EmailNotifications:{this.EmailNotifications}\n" +
                $"Jobs:{string.Join("\n", this.Jobs.Select(j => j.ToString()))}\n" +
                $"LogRoot:{this.LogRoot}\n" +
                $"ReceiverAddress:{this.ReceiverAddress}\n" +
                $"SenderAddress:{this.SenderAddress}\n" 
                ;
        }

        #endregion
    }
}