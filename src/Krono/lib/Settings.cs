namespace Krono
{

    public class Settings : SettingsBase
    {
        #region PROPERTIES

        public IEnumerable<Job> Jobs { get; set; } = new Job [] {};

        public bool Enabled { get; set; }

        public string ReceiverAddress { get; set; }
        
        public bool EmailNotifications { get; set; } 

        public string SenderAddress { get; set; }

        public string LogRoot { get; set; } = "/var/log/krono";

        #endregion 
    }
}