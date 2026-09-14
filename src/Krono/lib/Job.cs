namespace Krono
{
    public class Job
    {
        public string Name { get; set; }

        public string User { get; set; }

        public string Mask { get; set; }

        public string Command  { get; set; }

        public string LogPath  { get; set; }

        public string ErrorLogPath { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Verbose { get; set; }

        public string ReceiverAddress { get; set; }

        public override string ToString()
        {
            return "" +
                $"Name : {this.Name}\n" +
                $"User : {this.User}\n" +
                $"Mask : {this.Mask}\n" +
                $"Command : {this.Command}\n" +
                $"Enabled : {this.Enabled}\n" +
                $"LogPath : {this.LogPath}\n" +
                $"ErrorLogPath : {this.ErrorLogPath}\n" +
                "";
        }
    }
}