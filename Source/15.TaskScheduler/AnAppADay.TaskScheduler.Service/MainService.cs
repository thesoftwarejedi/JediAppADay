using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace AnAppADay.TaskScheduler.Service
{

    public partial class MainService : ServiceBase
    {
        private List<TaskLaunchInfo> _taskInfo;
        private Dictionary<Thread, ProcessWrapper> _threads;
        private bool _stop = false;
        private object pauseMutex = new object();
        private static Thread _serviceThread;

        public MainService()
        {
            InitializeComponent();
        }

        public void DebugStart()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            _threads = new Dictionary<Thread, ProcessWrapper>();
            _taskInfo = new List<TaskLaunchInfo>();

            try
            {
                //read in the config
                Tasks t = new Tasks();
                string configFile = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                configFile = configFile.Substring(0, configFile.LastIndexOf('/')) + "/Config.xml";
                t.ReadXml(configFile);
                if (t.Task == null || t.Task.Rows == null || t.Task.Rows.Count == 0)
                {
                    throw new Exception("No tasks found");
                }
                if (t.Schedule == null || t.Schedule.Rows == null || t.Schedule.Rows.Count == 0)
                {
                    throw new Exception("No schedules found");
                }

                //compute next launch time for each task
                foreach (Tasks.ScheduleRow row in t.Schedule)
                {
                    DateTime nextLaunch = GetNextLaunchTime(row);
                    TaskLaunchInfo l = new TaskLaunchInfo();
                    l.launchTime = nextLaunch;
                    l.schedule = row;
                    _taskInfo.Add(l);
                }
            }
            catch (Exception ex)
            {
                Program.WriteEvent("Error reading config: " + ex.Message + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);
                Stop();
                throw;
            }

            _serviceThread = new Thread(ServiceThreadRun);
            _serviceThread.Start();
        }

        private void ServiceThreadRun()
        {
            lock (pauseMutex)
            {
                while (!_stop)
                {
                    TimeSpan pause = new TimeSpan(0, 10, 0);
                    try
                    {
                        //launch pending tasks
                        DateTime now = DateTime.Now;
                        for (int i = 0; i < _taskInfo.Count; i++)
                        {
                            if (_taskInfo[i].launchTime < now)
                            {
                                LaunchTask(_taskInfo[i]);
                                TaskLaunchInfo l = _taskInfo[i];
                                l.launchTime = GetNextLaunchTime(_taskInfo[i].schedule);
                            }
                        }

                        //compute pause
                        now = DateTime.Now;
                        pause = new TimeSpan(100, 0, 0, 0, 0);
                        foreach (TaskLaunchInfo l in _taskInfo)
                        {
                            TimeSpan ti = (l.launchTime - now);
                            if (ti < pause)
                            {
                                pause = ti;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.WriteEvent("Error launching tasks: " + ex.Message, EventLogEntryType.Error);
                    }

                    //now, wait for that min time
                    if (pause > new TimeSpan(0))
                    {
                        Monitor.Wait(pauseMutex, pause);
                    }
                }
                _serviceThread = null; //we're meeeeeeeeelting......
                //kill all the processes in the running threads
                foreach (ProcessWrapper p in _threads.Values)
                {
                    p.Kill();
                }
                Thread.Sleep(3000);
                Monitor.PulseAll(pauseMutex);
                Stop();
            }
        }

        private void LaunchTask(TaskLaunchInfo l)
        {
            ProcessWrapper wrap = new ProcessWrapper(l.schedule);
            wrap.Complete += new ProcessWrapper.ProcessCompleteEventHandler(wrap_Complete);
            Thread t = new Thread(wrap.Go);
            _threads.Add(t, wrap);
            t.Start();
        }

        void wrap_Complete(object source)
        {
            _threads.Remove(Thread.CurrentThread); //sweet move
        }

        private DateTime GetNextLaunchTime(Tasks.ScheduleRow row)
        {
            DateTime now = DateTime.Now;

            string days = null;
            if (row.IsDaysNull())
            {
                days = "SMTWtFs";
            }
            else
            {
                days = row.Days;
            }
            DayOfWeek day = GetNextDayOfWeek(now, days);

            DateTime nextLaunch = DateTime.Now;
            //fast forward to next day to launch
            nextLaunch = FastForwardToDayOfWeek(day, nextLaunch);
            if (row.IsTimeNull())
            {
                //use interval
                nextLaunch = nextLaunch.AddSeconds(row.Interval);
            }
            else
            {
                //use time
                string[] time = row.Time.Split(':');
                int hour = int.Parse(time[0]);
                int min = int.Parse(time[1]);
                DateTime tempDate = new DateTime(nextLaunch.Year, nextLaunch.Month, nextLaunch.Day, hour, min, 0);
                if (tempDate < nextLaunch) {
                    tempDate = tempDate.AddDays(1);
                    DayOfWeek d = GetNextDayOfWeek(tempDate, days);
                    tempDate = FastForwardToDayOfWeek(d, tempDate);
                }
                nextLaunch = tempDate;
            }
            return nextLaunch;   
        }

        private static DateTime FastForwardToDayOfWeek(DayOfWeek day, DateTime nextLaunch)
        {
            while (nextLaunch.DayOfWeek != day)
            {
                nextLaunch = nextLaunch.AddDays(1).Date;
            }
            return nextLaunch;
        }

        private static DayOfWeek GetNextDayOfWeek(DateTime t, string days)
        {
            bool[] daysMarked = new bool[7];
            if (days.Contains("S")) daysMarked[0] = true;
            if (days.Contains("M")) daysMarked[1] = true;
            if (days.Contains("T")) daysMarked[2] = true;
            if (days.Contains("W")) daysMarked[3] = true;
            if (days.Contains("t")) daysMarked[4] = true;
            if (days.Contains("F")) daysMarked[5] = true;
            if (days.Contains("s")) daysMarked[6] = true;

            int i = (int)t.DayOfWeek;
            int cnt = 0;
            while (cnt < 7)
            {
                if (daysMarked[i]) break;
                i++;
                if (i > 6) i = 0;
            }
            DayOfWeek day = (DayOfWeek)i;
            return day;
        }

        protected override void OnStop()
        {
            lock (pauseMutex)
            {
                if (_serviceThread != null && _serviceThread.IsAlive)
                {
                    _stop = true;
                    //force the service thread to iterate and see it's supposed to stop
                    Monitor.PulseAll(pauseMutex);
                    //wait for the return signal that it's stopped, 15 secs max....
                    Monitor.Wait(pauseMutex, 15000);
                }
            }
        }

        private class TaskLaunchInfo
        {
            public DateTime launchTime;
            public Tasks.ScheduleRow schedule;
        }

    }

}
