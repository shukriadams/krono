//PORTER-WRAPPER!
namespace Krono.Porter_Packages {
//PORTER-WRAPPER!


using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Madscience_CustomEnvironmentArgs
{
    /// <summary>
    /// Loads env vars from an ".env" file found in app execution directory or in any parent directory. 
    /// Intended for development environments, but can in theory be used elsewhere.
    ///
    /// Env file content should be name=value, one per line.
    /// </summary>
    public class CustomEnvironmentArgs
    {
        public string FileName {get;set;} = ".env";

        public bool Verbose {get;set;} = false;

        public int Traversed  {get;set;}

        /// <summary>
        /// Maximum number of levels to crawl up parent directories to find .env file. Default is -1
        /// which is unlimited.
        /// </summary>
        public int MaximumTraversals {get;set;} = -1;

        /// <summary>
        /// Finds a .env file and loads its content as environment variables.
        /// </summary>
        public void FindAndApply()
        {
            string envArgFilePath = null;
    
            // Crawl up directory tree from app start dir, look for .env file until reach disk root.
            // Necessary because basedirectory varies between web and CLI app.
            DirectoryInfo currentPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            
            while (currentPath.Parent != null) 
            { 
                if (File.Exists(Path.Combine(currentPath.FullName, this.FileName))) 
                {
                    envArgFilePath = Path.Combine(currentPath.FullName, this.FileName);
                    break;
                }

                if (this.MaximumTraversals != -1 && this.Traversed >= this.MaximumTraversals)
                    break;

                currentPath = currentPath.Parent;
                this.Traversed ++;
            }

            if (envArgFilePath == null)
                return;

            if (this.Verbose)
                Console.WriteLine($"{this.FileName} file found at {envArgFilePath}");

            string fileContent = File.ReadAllText(envArgFilePath);
            fileContent = fileContent.Replace("\r\n", "\n");
            string[] args = fileContent.Split("\n");
            Regex envVarRegex = new Regex(@"(.*)?=(.*)");

            foreach(string arg in args)
            {
                Match match = envVarRegex.Match(arg);
                if (!match.Success)
                    continue;

                if (match.Groups.Count < 3)
                    continue;

                Environment.SetEnvironmentVariable(match.Groups[1].Value, match.Groups[2].Value);
                if (this.Verbose)
                    Console.WriteLine($"Set environment variable \"{match.Groups[1].Value}\"");
            }
                
        }
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!