using System.Reflection;
using System.IO;
using Krono.Porter_Packages.MadScience_ReflectionHelpers;
using Krono.Porter_Packages.Madscience_YamlLoader;

namespace Krono
{
    public class DaemonMode
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<Daemon> Daemons = new List<Daemon>();

        /// <summary>
        /// 
        /// </summary>
        public void Work(Settings settings)
        {
            if (!settings.Jobs.Any())
            {
                Console.WriteLine("No jobs defined in settings, exiting");
                return;
            }

            foreach(Job job in settings.Jobs)
            {
                if (!job.Enabled){
                    Console.WriteLine($"Job {job.Name} disabled, skipping start");
                    continue;
                }

                Daemon daemon = new Daemon(job, settings);
                daemon.Start();

                this.Daemons.Add(daemon);
            }
        }
    }    
}