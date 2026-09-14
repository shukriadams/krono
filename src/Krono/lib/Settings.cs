namespace Krono
{

    public class Settings
    {
        public IEnumerable<Job> Jobs { get; set; } = new Job [] {};

        public bool Enabled { get; set; }

        public string ReceiverAddress { get; set; }
        
        public string SenderAddress { get; set; }

        public string IndentCollection<T>(IEnumerable<T> content)
        {
            string t = string.Empty;

            foreach(object item in content)
            {
                string[] rows = item.ToString().Split("\n");
                foreach(string row in rows)
                    t += $"\t{row}\n";
            }

            return t;
        }

        public override string ToString()
        {
            return "" +
                $"Enabled : {this.Enabled}\n" +
                $"Jobs : \n" +
                $"{IndentCollection<Job>(this.Jobs)}";
        }
    }
}