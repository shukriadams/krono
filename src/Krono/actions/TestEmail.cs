using System.Reflection;
using System.Linq;
using System.IO;
using Krono.Porter_Packages.MadScience_ReflectionHelpers;
using Krono.Porter_Packages.Madscience_YamlLoader;

namespace Krono
{
    public class TestEmail
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work(string email, Settings settings)
        {
            IEmailAlert testSend = new Sendmail(new Email { 
                SenderAddress = settings.SenderAddress,
                ReceiverAddress = email,
                Subject = "Testing email send",
                Body = $"This was a simple email send test generated at {DateTime.Now}"
            });

            Response response = testSend.Send();
            if (response.Succeeded)
            {
                Console.WriteLine("Email test sent");
                return;
            }

            Console.WriteLine($"Error sending email : {response.Description}");
        }
    }    
}