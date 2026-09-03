using System.Reflection;
using Krono.Porter_Packages.MadScience_ReflectionHelpers;

namespace Krono
{
    public class Version
    {
        /// <summary>
        /// 
        /// </summary>
        public void Work()
        {
            string currentVersion = ResourceHelper.ReadStringResourceFromCallingAssembly("Krono.currentVersion.txt");
            Console.WriteLine($"version : {currentVersion}");
        }
    }    
}