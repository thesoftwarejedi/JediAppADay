using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Runtime;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace AnAppADay.CommandLineCSharp.ConsoleApp
{

    static class Program
    {

        private static string header = null;
        private static string footer = null;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    OuputHelp();
                    Environment.Exit(0);
                }

                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.CommandLineCSharp.ConsoleApp.Header.txt"))
                {
                    using (StreamReader read = new StreamReader(s))
                    {
                        header = read.ReadToEnd();
                    }
                }
                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.CommandLineCSharp.ConsoleApp.Footer.txt"))
                {
                    using (StreamReader read = new StreamReader(s))
                    {
                        footer = read.ReadToEnd();
                    }
                }

                string[] newArgs = CreateNewArgs(args);

                CompilerParameters param = new CompilerParameters();
                param.Evidence = AppDomain.CurrentDomain.Evidence;
                param.GenerateExecutable = false;
                param.GenerateInMemory = true;
                param.IncludeDebugInformation = false;
                param.ReferencedAssemblies.Add("mscorlib.dll");
                param.ReferencedAssemblies.Add("System.dll");
                param.ReferencedAssemblies.Add("System.Data.dll");
                param.ReferencedAssemblies.Add("System.Drawing.dll");
                param.ReferencedAssemblies.Add("System.Xml.dll");
                param.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                param.TreatWarningsAsErrors = false;

                string code = File.ReadAllText(args[0]);
                code = ApplyHeaderFooter(code);

                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                CompilerResults cr = codeProvider.CompileAssemblyFromSource(param, code);
                if (cr.Errors != null && cr.Errors.Count > 0)
                {
                    OutputErrors(cr.Errors);
                    Environment.Exit(-1);
                }
                Assembly ass = cr.CompiledAssembly;
                object o = ass.CreateInstance("AnAppADay.CommandLineCSharp.CodeGen.MainClass");
                Type t = o.GetType();
                MethodInfo mi = t.GetMethod("Main");
                mi.Invoke(null, new object[] { newArgs });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private static string ApplyHeaderFooter(string code)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(header);
            sb.Append(code);
            sb.Append(footer);
            return sb.ToString();
        }

        private static string[] CreateNewArgs(string[] args)
        {
            if (args.Length == 1) return null;
            string[] newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            return newArgs;
        }

        private static void OutputErrors(CompilerErrorCollection compilerErrorCollection)
        {
            Console.WriteLine("Compilation errors:");
            foreach (CompilerError e in compilerErrorCollection)
            {
                Console.WriteLine(e.ErrorText);
            }
        }

        private static void OuputHelp()
        {
            Console.WriteLine("usage: clcs <csharpfile>");
            Console.WriteLine("Where <csharpfile> is the name of a file which contains c# code");
            Console.WriteLine("The file should start with \"{\" which designates the beginning of the main method");
        }
    }
}

