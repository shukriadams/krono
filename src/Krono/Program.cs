using System;
using Krono.Porter_Packages.Madscience_CommandLineSwitches;
using Krono.Porter_Packages.Madscience_CustomEnvironmentArgs;

namespace Krono
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                Console.WriteLine("Krono - the portable cron runner");

                CustomEnvironmentArgs customEnvironmentArgs = new CustomEnvironmentArgs { 
                    Verbose = true
                };
                customEnvironmentArgs.FindAndApply();

 
                CommandLineSwitches switches = new CommandLineSwitches();
                
                switches.Add(new Argument("version", typeof(string)) { LongName = "version", ShortName = "v", IsExclusive = true });
                switches.Add(new Argument("mailtest", typeof(string)) { LongName = "mailtest", ShortName = "m", IsExclusive = true });
                switches.Add(new Argument("daemon", typeof(string)) { LongName = "daemon", ShortName = "d", IsExclusive = true });
                switches.Add(new Argument("receiver", typeof(string)) { LongName = "receiver", ShortName = "r"});
    
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
                if (switches.IsSet("daemon"))
                    command = "daemon";
                else if (switches.IsSet("mailtest"))
                    command = "mailtest";

                // do version before loading settings, it should be possible to read version without having
                // settings configured
                if (command == "version")
                {
                    Version version = new Version();
                    version.Work();
                    return;
                }
                
                // from here on, all functions rely on settings, so load settings now
                SettingsLoader settingsLoader = new SettingsLoader();
                SettingsLoadResponse settingsReponse = settingsLoader.Load();
                if (!settingsReponse.Succeeded)
                {
                    Console.WriteLine("Config error:");
                    Console.WriteLine(settingsReponse.Description);
                    Console.WriteLine(settingsReponse.Exception);
                    return;
                }

                if (command == "mailtest")
                {
                    Console.WriteLine(switches.Get<string>("receiver"));
                   
                    if (!switches.IsSet("receiver"))
                    {
                        Console.WriteLine("Email testing requires --receiver <EMAIL> ");
                        return;
                    }
                    
                    TestEmail testEmail = new TestEmail();
                    testEmail.Work(switches.Get<string>("receiver"), settingsReponse.Settings);
                    return;
                    
                }

                if (command == "daemon")
                {
                    Console.WriteLine("Starting in daemon mode");
                    DaemonMode dm = new DaemonMode();
                    dm.Work(settingsReponse.Settings);
                }

                if (command == null)
                {
                    Console.WriteLine("no command set. Use --daemon|-d to start in daemon mode.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}