using Krono.Porter_Packages.Cronos;
using Krono.Porter_Packages.MadScience_Shell;
using System.IO;
using Microsoft.Extensions.Logging;

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

        private ILogger<Daemon> _logger;

        public Daemon(Job job, Settings settings)
        {
            _job = job;
            _settings = settings;
            LogProvider<Daemon> logProvider = new LogProvider<Daemon>();
            _logger = logProvider.Create<Daemon>(job.Name);
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
            
            _logger.LogInformation($"Daemon starting");

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
            
                        _logger.LogInformation($"Run starting");

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
                        
                        _logger.LogInformation($"Run ended with result {result}");
                        
                        if(_settings.EmailNotifications && !string.IsNullOrEmpty(_job.ReceiverAddress) && (result !=0 || _job.Verbose))
                        {
                            _logger.LogInformation("Processing email ... ");

                            string subject = $"Job {_job.Name} passed.\n";
                            string body = "nothing to report";

                            if (result != 0)
                            {
                                subject = $"Job {_job.Name} failed";
                                body = $"An error occurred, last log was: \n\n\n{string.Join("\n", errors)}";
                            }

                            IEmailAlert alert = new Sendmail(new Email { 
                                SenderAddress = _settings.SenderAddress,
                                ReceiverAddress = _job.ReceiverAddress,
                                Subject = subject,
                                Body = body
                            });

                            Response sendResponse = alert.Send();
                            if (sendResponse.Succeeded)
                            {
                                _logger.LogInformation("Notification email sent");
                            }
                            else 
                            {
                                _logger.LogError($"Failed to send notification email sent {sendResponse.Description}");
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unexpected error");
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(5000); // recheck cron tick every 5 secs
                    }
                }

                _logger.LogInformation($"Daemon for {_job.Name} exiting");

            }).Start();
        }

        public void Stop()
        {
            _run = false;
        }
    }
}