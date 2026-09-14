using System.Reflection;
using System.IO;
using Krono.Porter_Packages.MadScience_ReflectionHelpers;
using Krono.Porter_Packages.Madscience_YamlLoader;

namespace Krono
{

    public class SettingsLoader
    {
        public SettingsLoadResponse Load()
        {
            // check if config file exists
            string filePath = "./test.yml"; // 

            if (!File.Exists(filePath))
                return new SettingsLoadResponse { Description = "Config file not found, exiting"};

            string rawYaml = File.ReadAllText(filePath);

            YamlLoader<Settings> loader = new YamlLoader<Settings>
            {
                EnableTokenSubstitution = true,
                OnLogStatus = (string log)=>{ Console.WriteLine(log); },
                OnLogInfo = (string log)=>{ Console.WriteLine(log); }
            };

            YamlLoadResponse<Settings> response = loader.FromString(rawYaml);
            if (!response.Succeeded)
                return new SettingsLoadResponse { 
                    Description = $"Failed to load yml from file {filePath}",
                    Exception = response.Exception
                };

            if (string.IsNullOrEmpty(response.Payload.SenderAddress))
            {
                response.Payload.EmailNotifications = false;
                Console.WriteLine("SenderAddress not set, email notifications force disabled.");
            }

            // process settings, applie defaults etc
            foreach(Job job in response.Payload.Jobs)
                if (string.IsNullOrEmpty(job.ReceiverAddress))
                    // set fallback email address
                    job.ReceiverAddress = response.Payload.ReceiverAddress;

            return new SettingsLoadResponse{ Settings = response.Payload };
        }
    }
}