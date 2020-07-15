using System;
using System.IO;
using System.Security.Principal;

namespace ForwardNotifierSetup
{
    class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Console.Title = $"{Setup.AppName} Setup";

            if (!IsAdministrator())
            {
                Console.WriteLine("You must be administrator in order to run the setup. Press any key to continue. . .");
                Console.ReadKey(true);
                return -1;
            }

            if (args.Length == 0)
            {
                if (!Setup.IsPythonInstalled())
                {
                    Setup.GetPython();
                    Setup.InstallPython();
                }

                Setup.GetModules();

                Setup.GetInstallServer();
            }

            Console.ReadKey(true);

            return 0;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("A unhandled exception has occurred, report this to the developer.");
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.ReadKey(true);
            Environment.Exit(-1);
        }

        static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
