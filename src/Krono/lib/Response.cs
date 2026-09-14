namespace Krono
{
    public class Response
    {
        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }

        public string Description { get; set; }
    }
}