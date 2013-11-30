using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.QuickKill.ConsoleApp
{

    class Program
    {

        static void Main(string[] args)
        {
            bool quiet = false;
            bool help = false;
            string file = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-q":
                        quiet = true;
                        break;
                    case "-f":
                        file = args[++i];
                        break;
                    case "-?":
                        help = true;
                        break;
                }
            }
            if (help) {
                Console.WriteLine("Kills all processes except those in the exclusion file");
                Console.WriteLine("Usage: AnAppADay.QuickKill.ConsoleApp.exe [-q] [-f <filename>] [-?]");
                Console.WriteLine("-q = quiet mode");
                Console.WriteLine("-f <filename> = read specified exclusion file");
                Console.WriteLine("-? = display this help");
                Console.WriteLine("Default file is AnAppADay.QuickKill.ConsoleApp.jedi");
                Environment.Exit(0);
            }
            if (file == null)
            {
                file = "AnAppADay.QuickKill.ConsoleApp.jedi";
            }
            System.Console.WriteLine("Will read file: " + file);
            Console.WriteLine();
            string[] exclusions = null;
            try
            {
                exclusions = File.ReadAllLines(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading input file: " + ex.Message);
                Environment.Exit(-1);
            }
            //convert to lower
            for (int i = 0; i < exclusions.Length; i++)
            {
                exclusions[i] = exclusions[i].ToLower();
            }
            //sort
            Array.Sort(exclusions);
            Console.WriteLine("Will exclude processes:");
            foreach (string e in exclusions) 
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
            Console.WriteLine("Will KILL processes:");
            Process[] processes = Process.GetProcesses();
            List<Process> killList = new List<Process>();
            foreach (Process p in processes)
            {
                if (!(p.ProcessName == "System" || p.ProcessName == "Idle"))
                {
                    if (Array.BinarySearch(exclusions, p.MainModule.ModuleName.ToLower()) < 0)
                    {
                        Console.WriteLine(p.MainModule.ModuleName);
                        killList.Add(p);
                    }
                }
            }
            Console.WriteLine();
            if (!quiet)
            {
                Console.WriteLine("Are you sure? (y/n)");
                if (Console.ReadKey(true).Key != ConsoleKey.Y)
                {
                    Environment.Exit(-1);
                }
            }
            foreach (Process p in killList)
            {
                Console.Write("Killing " + p.MainModule.ModuleName + "... ");
                try
                {
                    p.Kill();
                    Console.WriteLine("Success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            Console.WriteLine();
            Console.WriteLine("Completed");
        }

    }

}
