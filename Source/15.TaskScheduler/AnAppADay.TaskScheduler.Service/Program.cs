using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;

namespace AnAppADay.TaskScheduler.Service
{

    static class Program
    {

        const string source = "AnAppADay.com Task Scheduler Service";

        static void Main()
        {
            try
            {
                //windows vista beta 2 bombs here because of permissions?!?!?!?!?
                //it shouldn't need read permission to the "security" log as
                //the error says it does.  Only solution is to create in code
                //which I'm not doing as I beleive it to be specific to Vista B2

                //besides, installer should do this
                EventLog.CreateEventSource(source, "Application");
            }
            catch { }

            MainService service = new MainService();

#if(DEBUG)
            service.DebugStart();
#else
            ServiceBase.Run(service);
#endif
        }

        internal static void WriteEvent(string message, EventLogEntryType type)
        {
            EventLog.WriteEntry(source, message, type);
        }

    }

}