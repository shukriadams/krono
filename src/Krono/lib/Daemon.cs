using Krono.Porter_Packages.Cronos;
using Krono.Porter_Packages.MadScience_Shell;
using System.IO;

namespace Krono
{
    public class Daemon
    {
        private CronExpression _cronExpression;
        
        private bool _run;
        
        private bool _busy;
        
        private Job _job;
        
        private Settings _settings;

        private DateTime _lastRun;

        public Daemon(Job job, Settings settings)
        {
            _job = job;
            _settings= settings;
        }

        public void Start()
        {
            _cronExpression = CronExpression.Parse(_job.Mask);
            _lastRun = DateTime.UtcNow;
            _run = true;

            if (!string.IsNullOrEmpty(_job.LogPath))
            {
                string baseDir = Path.GetDirectoryName(_job.LogPath);
                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);
            }

            if (!string.IsNullOrEmpty(_job.ErrorLogPath))
            {
                string baseDir = Path.GetDirectoryName(_job.ErrorLogPath);
                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);
            }

            Console.WriteLine($"Starting daemon for : {_job.Name}");

            new Thread(async delegate ()
            {
                while (_run)
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

                        Shell shell = new Shell(_job.Command);
                        List<string> errors = new List<string>();

                        if (!string.IsNullOrEmpty(_job.LogPath))
                            shell.OnInfoBuffered = (IEnumerable<string> log) => { 
                                File.AppendAllLines(_job.LogPath, log ); 
                            };

                        if (!string.IsNullOrEmpty(_job.ErrorLogPath))
                            shell.OnErrorBuffered = (IEnumerable<string> log) =>{ 
                                File.AppendAllLines(_job.ErrorLogPath, log ); 
                                errors.AddRange(log);
                            };

                        int result = shell.Run();

                        if(!string.IsNullOrEmpty(_job.ReceiverAddress) && (result !=0 || _job.Verbose))
                        {
                            string subject = $"Job {_job.Name} passed.\n";
                            string body = "";

                            if (result != 0)
                            {
                                subject = $"Job {_job.Name} failed.\n" +
                                    $"{string.Join("\n", errors)}";
                            }

                            IEmailAlert alert = new Sendmail(new Email { 
                                SenderAddress = _settings.SenderAddress,
                                ReceiverAddress = _settings.ReceiverAddress,
                                Subject = subject,
                                Body = body
                            });

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

                Console.WriteLine($"Daemon for {_job.Name} exiting");

            }).Start();
        }

        public void Stop()
        {
            _run = false;
        }
    }
}