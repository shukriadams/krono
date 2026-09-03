//PORTER-WRAPPER!
namespace Krono.Porter_Packages {
//PORTER-WRAPPER!


using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MadScience_Shell
{
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
                                this.OutLines.Add(e.Data);
                        }
                        catch (Exception ex)
                        {
                            this.ErrLines.Add(e.ToString());
                        }
                    };

                    cmd.ErrorDataReceived += (sender, e) =>
                    {
                        try
                        {
                            if (e.Data == null)
                                errorWaitHandle.Set();
                            else
                                this.ErrLines.Add(e.Data);
                        }
                        catch (Exception ex)
                        {
                            this.ErrLines.Add(ex.ToString());
                        }
                    };

                    cmd.Start();
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    if (cmd.WaitForExit(this.Timeout) && outputWaitHandle.WaitOne(this.Timeout) && errorWaitHandle.WaitOne(this.Timeout))
                        return cmd.ExitCode;
                    else
                    {
                        this.ErrLines.Add($"Timed out on command : {_command} after {this.Timeout} ms");
                        return 1;
                    }
                        
                }
            }
            else
            {
                cmd.Start();
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();

                while (!cmd.StandardOutput.EndOfStream)
                {
                    string line = cmd.StandardOutput.ReadLine();
                    this.OutLines.Add(line);
                }

                while (!cmd.StandardError.EndOfStream)
                {
                    string line = cmd.StandardError.ReadLine();
                    this.ErrLines.Add(line);
                }

                return cmd.ExitCode;
            }
        }

        #endregion
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!