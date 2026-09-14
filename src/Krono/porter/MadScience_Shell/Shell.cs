//PORTER-WRAPPER!
namespace Krono.Porter_Packages {
//PORTER-WRAPPER!


using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MadScience_Shell
{
    public delegate void LogEvent(string logText);

    public delegate void LogEventBuffered(IEnumerable<string> logRows);

    /// <summary>
    /// Crudely-cross platform shell wrapper. Tries to abstract away most of the .net quirks of running 
    /// shell commands, especially on linux. Can do with improvements, but has worked in production 
    /// environments for years.
    /// </summary>
    public class Shell
    {
        #region FIELDS

        private readonly string _command;

        public ShellType ShellType { get; set; }

        #endregion

        #region PROPERTIES

        public List<string> OutLines { get; private set; } = new List<string>();

        public List<string> ErrLines { get; private set; } = new List<string>();

        public LogEvent OnInfo;

        public LogEvent OnError;

        public LogEventBuffered OnInfoBuffered;

        public LogEventBuffered OnErrorBuffered;
        
        public int LogBufferSize {get;set;} = 10;

        public string Out
        {
            get
            {
                return string.Join("\n", OutLines);
            }  
        }

        public string Err
        {
            get
            {
                return string.Join("\n", ErrLines);
            }  
        }

        // in milliseconds
        public int Timeout { get; set; } = 10000;
        
        public string WorkingDirectory { get; set; }
        
        #endregion

        #region CTORS

        public Shell(string command)
        {
            _command = command;

            // fallback to sane defaults for windows vs others. This can be overridden before running.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ShellType = ShellType.Cmd;
            else
                ShellType = ShellType.Sh;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Executes shell command. Does not throw exceptions - check if return code is 0 to determine if passed,
        /// check Err or ErrLines for details.
        /// </summary>
        /// <returns></returns>
        public int Run()
        {
            Process cmd = new Process();

            if (this.ShellType == ShellType.Sh)
            {
                cmd.StartInfo.FileName = "sh";
                cmd.StartInfo.Arguments = $"-c \"{_command}\"";
            }
            else
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/k {_command}";
            }

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            if (!string.IsNullOrEmpty(this.WorkingDirectory))
                cmd.StartInfo.WorkingDirectory = this.WorkingDirectory;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    cmd.OutputDataReceived += (sender, e) =>
                    {
                        try
                        {
                            if (e.Data == null)
                                outputWaitHandle.Set();
                            else
                            {
                                if (this.OnInfo != null)
                                    this.OnInfo.Invoke(e.Data);

                                this.Log(e.Data);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (this.OnError != null)
                                this.OnError.Invoke(e.ToString());

                            this.Error(e.ToString());
                        }
                    };

                    cmd.ErrorDataReceived += (sender, e) =>
                    {
                        try
                        {
                            if (e.Data == null)
                                errorWaitHandle.Set();
                            else
                            {
                                if (this.OnError != null)
                                    this.OnError.Invoke(e.Data);

                                this.Error(e.Data);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (this.OnError != null)
                                this.OnError.Invoke(ex.ToString());

                            this.Error(ex.ToString());
                        }
                    };

                    cmd.Start();
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    if (cmd.WaitForExit(this.Timeout) && outputWaitHandle.WaitOne(this.Timeout) && errorWaitHandle.WaitOne(this.Timeout))
                    {
                        this.FlushLogBuffer();
                        this.FlushErrorBuffer();
                        return cmd.ExitCode;
                    }
                    else
                    {
                        this.Error($"Timed out on command : {_command} after {this.Timeout} ms");

                        this.FlushLogBuffer();
                        this.FlushErrorBuffer();

                        return 1;
                    }
                        
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmd.Start();
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();

                while (!cmd.StandardOutput.EndOfStream)
                {
                    string line = cmd.StandardOutput.ReadLine();

                    if (this.OnInfo != null)
                        this.OnInfo.Invoke(line);

                    this.Log(line);
                }

                while (!cmd.StandardError.EndOfStream)
                {
                    string line = cmd.StandardError.ReadLine();

                    if (this.OnError != null)
                        this.OnError.Invoke(line);

                    this.Error(line);
                }
                
                this.FlushLogBuffer();
                this.FlushErrorBuffer();

                return cmd.ExitCode;
            }
            else
            {
                throw new Exception($"Unsupported os platform");
            }
        }

        private void FlushLogBuffer()
        {
            if (this.OnInfoBuffered == null)
                return;

            lock(this.OutLines)
            {
                this.OnInfoBuffered.Invoke(this.OutLines.Take(this.OutLines.Count()));
                this.OutLines = new List<string>();
            }
        }

        private void FlushErrorBuffer()
        {   
            if (this.OnErrorBuffered == null)
                return;

            lock(this.ErrLines)
            {
                this.OnErrorBuffered.Invoke(this.ErrLines.Take(this.ErrLines.Count()));
                this.ErrLines = new List<string>();
            }
        }

        private void Log(string text)
        {
            lock(this.OutLines)
                this.OutLines.Add(text);

            if (this.OnInfoBuffered != null && this.OutLines.Count() > this.LogBufferSize)
                this.FlushLogBuffer();
        }

        private void Error(string text)
        {
            lock(this.ErrLines)
                this.ErrLines.Add(text);

            if (this.OnErrorBuffered != null && this.ErrLines.Count() > this.LogBufferSize)
                lock(this)
                    this.OnErrorBuffered.Invoke(this.ErrLines.Take(this.ErrLines.Count()));
        }

        #endregion
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!