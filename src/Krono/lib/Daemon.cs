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

        public void Start(Job job)
        {
            _cronExpression = CronExpression.Parse(job.Mask);
            _lastRun = DateTime.UtcNow;
            _running = true;

            Console.WriteLine($"starting {job.Name}");

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

                        Console.WriteLine($"running {job.Name}");

                        _busy = true;
                        _lastRun = DateTime.UtcNow;

                        Shell shell = new Shell(job.Command);
                        int result = shell.Run();

                        // write output to log
                        if (result != 0)
                        {
                            // do error stuff here
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"Unhandled exception : {ex}");
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(5000); // recheck cron tick every 5 secs
                    }
                }

                Console.WriteLine($"Daemon for {job.Name} exiting");

            }).Start();
        }
    }
}