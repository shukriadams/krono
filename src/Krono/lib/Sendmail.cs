using Krono.Porter_Packages.MadScience_Shell;

namespace Krono
{
    public class Sendmail : IEmailAlert
    {
        private Email _email;

        public Sendmail(Email email)
        {
            _email = email;
        }

        public Response Test()
        {
            // ensure that sendmail is installed
            Shell shell = new Shell("sendmail -v");
            int result = shell.Run();
            if (result != 0)
                return new Response {
                    Description = $"Sendmail -v check failed, sendmail likely not installed. Got {result} back, {shell.Err}"
                };

            return new Response { 
                Succeeded = true 
            };
        }

        public Response Send()
        {
            File.WriteAllText(
                "/tmp/sendmail_test.txt", 
                $"From: {_email.SenderAddress}\n" +
                $"Subject: {_email.Subject}\n\n" +
                $"{_email.Body}");

            Shell shell = new Shell($"ssmtp {_email.ReceiverAddress} < /tmp/sendmail_test.txt");
            int result = shell.Run();
            if (result != 0)
                return new Response {
                    Description = $"Sendmail send failed, got {result} back : {shell.Err}"
                };

            return new Response{ Succeeded = true };
        }
    }
}