using Krono.Porter_Packages.Cronos;
using Krono.Porter_Packages.MadScience_Shell;

namespace Krono
{
    public class Daemon
    {
        private CronExpression _cronExpression;
        
        private bool _running;
        
        private bool _busy;
        
        private DateTime _lastRun;

        public void Start(string cronmask, string command)
        {
            _cronExpression = CronExpression.Parse(cronmask);
            _lastRun = DateTime.UtcNow;

            new Thread(async delegate ()
            {
                while (_running)
                {
                    try
                    {
                        if (_busy)
                            continue;

                        DateTime? nextUtc = _cronExpression.GetNextOccurrence(_lastRun);
                        if (nextUtc > DateTime.UtcNow)
                            continue;

                        _busy = true;
                        _lastRun = DateTime.UtcNow;
                        Shell shell = new Shell(command);

                        int result = shell.Run();
                        // write output to log
                        if (result != 0)
                        {
                            // do error stuff here
                        }
                    }
                    catch (Exception ex)
                    {
                        //_log.LogError(ex, $"Unhandled daemon exception from {work.Method.DeclaringType.Name}");
                        Console.Write($"Unhandled exception : {ex}");
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(60000); // recheck cron tick every minute, no need to check more frequently given minute resolution of cronmask
                    }
                }
            }).Start();
        }
    }
}