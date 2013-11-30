using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.TaskScheduler.Service
{

    class ProcessWrapper
    {

        private Tasks.ScheduleRow _schedule;
        private StreamWriter _outWriter;
        private StreamWriter _errWriter;
        private Process _osProcess;

        public ProcessWrapper(Tasks.ScheduleRow schedule)
        {
            _schedule = schedule;
        }

        public delegate void ProcessCompleteEventHandler(object source);
        public event ProcessCompleteEventHandler Complete;

        public void Go()
        {
            try
            {
                //setup writers
                string nowString = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                string outFile = _schedule.TaskRow.IsStdOutNull() ? null : _schedule.TaskRow.StdOut;
                if (outFile != null)
                {
                    if (outFile.Contains("{0}"))
                    {
                        outFile = string.Format(outFile, nowString);
                    }
                    int prefix = 0;
                    string tempFile = outFile;
                    while (File.Exists(outFile))
                    {
                        outFile = (prefix++) + tempFile;
                    }
                    _outWriter = new StreamWriter(new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
                }
                string errFile = _schedule.TaskRow.IsStdErrNull() ? null : _schedule.TaskRow.StdErr;
                if (errFile != null)
                {
                    if (errFile.Contains("{0}"))
                    {
                        errFile = string.Format(errFile, nowString);
                    }
                    if (errFile != outFile)  //if they are equal they will share event handlers below
                    {
                        _errWriter = new StreamWriter(new FileStream(errFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
                    }
                }
                //setup and start process
                string args = _schedule.TaskRow.IsArgsNull() ? "" : _schedule.TaskRow.Args;
                ProcessStartInfo psi = new ProcessStartInfo(_schedule.TaskRow.Execute, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                _osProcess = new Process();
                //setup file writers
                if (_outWriter != null)
                {
                    psi.RedirectStandardOutput = true;
                    _osProcess.OutputDataReceived += new DataReceivedEventHandler(_osProcess_OutputDataReceived);
                }
                if (_errWriter != null || (errFile == outFile && errFile != null))
                {
                    psi.RedirectStandardError = true;
                    if (errFile == outFile)
                    {
                        //share the output file
                        _osProcess.ErrorDataReceived += new DataReceivedEventHandler(_osProcess_OutputDataReceived);
                    }
                    else
                    {
                        _osProcess.ErrorDataReceived += new DataReceivedEventHandler(_osProcess_ErrorDataReceived);
                    }
                }
                _osProcess.StartInfo = psi;
                //start!
                _osProcess.Start();
                if (_outWriter != null)
                {
                    _osProcess.BeginOutputReadLine();
                }
                if (_errWriter != null || errFile == outFile)
                {
                    _osProcess.BeginErrorReadLine();
                }
                if (_schedule.TaskRow.IsKillAfterNull())
                {
                    _osProcess.WaitForExit();
                }
                else
                {
                    _osProcess.WaitForExit((int)_schedule.TaskRow.KillAfter * 1000);
                }
                if (IsRunning())
                {
                    Kill();
                }
                //let the output flush, appears to be no clean way to handle
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Program.WriteEvent("Error starting process: " + ex.Message + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);
            }
            finally
            {
                CleanupStreams();
                if (Complete != null)
                {
                    Complete(this);
                }
            }
        }

        public bool IsRunning()
        {
            if (_osProcess != null && !_osProcess.HasExited)
            {
                return true;
            }
            return false;
        }

        public void Kill()
        {
            if (_osProcess != null && !_osProcess.HasExited)
            {
                if (_outWriter != null)
                {
                    lock (_outWriter)
                    {
                        _outWriter.WriteLine();
                        _outWriter.WriteLine("--------- TASK KILLED AT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ---------");
                    }
                }
                _osProcess.Kill();
            }
        }

        void _osProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_outWriter)
            {
                _outWriter.WriteLine(e.Data);
            }
        }

        void _osProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_errWriter)
            {
                _errWriter.WriteLine(e.Data);
            }
        }

        void CleanupStreams()
        {
            if (_outWriter != null)
            {
                WriteFooter(_outWriter);
                try { _outWriter.Close(); }
                catch { } 
                try { _outWriter.Dispose(); }
                catch { } 
                _outWriter = null;
            }
            if (_errWriter != null)
            {
                WriteFooter(_errWriter);
                try { _errWriter.Close(); }
                catch { }
                try { _errWriter.Dispose(); }
                catch { } 
                _errWriter = null;
            }
        }

        private void WriteFooter(StreamWriter write)
        {
            write.WriteLine();
            write.WriteLine("--------- TASK FINISHED AT " + _osProcess.ExitTime.ToString("yyyy-MM-dd HH:mm:ss") + " ---------");
            write.WriteLine("--------- RETURN CODE: " + _osProcess.ExitCode + " ---------");
        }

    }

}
