using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AnAppADay.TimeManagement.WinApp
{
    static class Program
    {

        static NotifyIcon ntfy;
        static Thread _t;
        static StreamWriter write;
        static object writeMutex = new object();

        const uint GA_PARENT = 1;
        const uint GA_ROOT = 2;
        const uint GA_ROOTOWNER = 3;

        static IntPtr lastWin = IntPtr.Zero;
        static RecordEntry lastEntry;

        internal static int pollRate = 5000;

        private static bool STOP = false;
        private static IntPtr desktopWin;

        internal static string filename;

        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                desktopWin = GetDesktopWindow();

                //read config file
                try
                {
                    using (FileStream iniIn = new FileStream("AnAppADay.TimeManagement.WinApp.jedi", FileMode.Open, FileAccess.Read))
                    {
                        using (StreamReader iniRead = new StreamReader(iniIn))
                        {
                            pollRate = Int32.Parse(iniRead.ReadLine());
                            filename = iniRead.ReadLine();
                        }
                    }
                }
                catch (Exception)
                {
                    filename = "TimeManagement.csv";
                }

                //setup the output file
                OpenOutputFile();

                //setup the systray
                ntfy = new NotifyIcon();
                MenuItem[] menuItems = new MenuItem[3];
                menuItems[0] = new MenuItem("Options");
                menuItems[0].Click += new EventHandler(Options_Select);
                menuItems[1] = new MenuItem("About");
                menuItems[1].Click += new EventHandler(About_Select);
                menuItems[2] = new MenuItem("Exit");
                menuItems[2].Click += new EventHandler(Exit_Select);
                ContextMenu menu = new ContextMenu(menuItems);
                ntfy.ContextMenu = menu;
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
                ntfy.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                ntfy.Visible = true;

                //start watching the apps
                _t = new Thread(RecordApplications);
                _t.Start();

                //start the gui msg loop
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unknown error occurred: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        internal static void OpenOutputFile()
        {
            lock (writeMutex)
            {
                try
                {
                    if (write != null)
                    {
                        write.Close();
                    }
                    write = new StreamWriter(new FileStream(filename, FileMode.Append, FileAccess.Write));
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't open output file, will use default");
                    write = new StreamWriter(new FileStream("TimeManagement.csv", FileMode.Append, FileAccess.Write, FileShare.Read));
                }
                write.WriteLine();
            }
        }

        static void Options_Select(object sender, EventArgs e)
        {
            OptionsForm fmr = new OptionsForm();
            fmr.ShowDialog();
        }

        static void About_Select(object sender, EventArgs e)
        {
            Process.Start("http://www.AnAppADay.com/");
        }

        static void Exit_Select(object sender, EventArgs e)
        {
            ntfy.Visible = false;
            ntfy.Dispose();
            STOP = true;
            //let the other thread exit
            Thread.Sleep(pollRate + 1000);
            write.Close();
            Application.Exit();
            System.Environment.Exit(0);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        static void RecordApplications()
        {
            while (!STOP)
            {
                try
                {
                    //get the process window name and exe name
                    IntPtr tempWin = GetForegroundWindow();
                    IntPtr win = tempWin;
                    while (tempWin != IntPtr.Zero && tempWin != desktopWin)
                    {
                        win = tempWin;
                        tempWin = GetAncestor(tempWin, GA_PARENT);
                    }
                    StringBuilder sb = new StringBuilder();
                    GetWindowText(win, sb, 150);
                    string title = sb.ToString();
                    if (win != lastWin || title != lastEntry.title)
                    {
                        lastWin = win;
                        if (lastEntry.procName != null)
                        {
                            lastEntry.timeSpan = DateTime.Now - lastEntry.dateTime;
                            WriteLastEntry();
                        }
                        uint procId;
                        GetWindowThreadProcessId(win, out procId);
                        Process p = Process.GetProcessById((int)procId);
                        string procName = p.ProcessName;
                        //now title is the win title and procName is the EXE
                        lastEntry.dateTime = DateTime.Now;
                        lastEntry.procName = procName;
                        lastEntry.title = title;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message + Environment.NewLine + "The application will retry..." + Environment.NewLine + ex.StackTrace + Environment.NewLine);
                }
                finally
                {
                    Thread.Sleep(pollRate);
                }
            }
        }

        private static void WriteLastEntry()
        {
            lock (writeMutex)
            {
                write.WriteLine(lastEntry);
                write.Flush();
            }
        }
    }

    struct RecordEntry
    {
        public DateTime dateTime;
        public string procName;
        public string title;
        public TimeSpan timeSpan;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dateTime.ToString("MM/dd/yyyy"));
            sb.Append(",");
            sb.Append(dateTime.ToString("HH:mm:ss"));
            sb.Append(",");
            sb.Append(procName);
            sb.Append(",");
            sb.Append(title);
            sb.Append(",");
            sb.Append(timeSpan.TotalMinutes);
            return sb.ToString();
        }
    }
}
