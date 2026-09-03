using System;
using Krono.Porter_Packages.Madscience_CommandLineSwitches;

namespace Krono
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                Console.WriteLine("Krono");

                CommandLineSwitches switches = new CommandLineSwitches();
                
                switches.Add(new Argument("version", typeof(string)) { LongName = "version", ShortName = "v", IsExclusive = true });
    
                BindResponse bindResponse = switches.Bind(
                    args, 
                    validate : true);

                if (!bindResponse.Succeeded)
                {
                    Console.WriteLine($"Error :\n{bindResponse.Description}");
                    Environment.Exit(1);
                }

                string command = null;
                if (switches.IsSet("version"))
                    command = "version";

                if (command == "version")
                {
                    Version version = new Version();
                    version.Work();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}