using System.Reflection;
using System.IO;
using Krono.Porter_Packages.MadScience_ReflectionHelpers;
using Krono.Porter_Packages.Madscience_YamlLoader;

namespace Krono
{
    public class DaemonMode
    {
        public IList<Daemon> Daemons = new List<Daemon>();
        /// <summary>
        /// 
        /// </summary>
        public void Work()
        {
            // check if config file exists
            string filePath = "./test.yml";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Config file not found, exiting");
                return;
            }

            string rawYaml = File.ReadAllText(filePath);

            YamlLoader<Settings> loader = new YamlLoader<Settings>
            {
                EnableTokenSubstitution = true,
                OnLogStatus = (string log)=>{ Console.WriteLine(log); },
                OnLogInfo = (string log)=>{ Console.WriteLine(log); }
            };

            YamlLoadResponse<Settings> response = loader.FromString(rawYaml);
            if (!response.Succeeded)
            {
                Console.WriteLine("Failed to load yml from file");   
                Console.WriteLine(response.Description);
                Console.WriteLine(response.Exception);
                return;
            }

            // Console.WriteLine(response.Payload);
            foreach(Job job in response.Payload.Jobs)
            {
                Daemon daemon = new Daemon();
                daemon.Start(job);

                this.Daemons.Add(daemon);
            }
        }
    }    
}