using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace ForwardNotifierSetup
{
    class Setup
    {
        private static string PythonFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Programs\Python";

        private static readonly string ProgramsFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Programs";

        public static readonly string AppName = "ForwardNotifier";

        private static readonly string PipCommand = "install win10toast Pillow requests flask";

        private static readonly string ServerURL = "https://raw.githubusercontent.com/Greg0109/ForwardNotifier/master/ForwardNotifier%20Client%20Tools/Crossplatform%20Server/ForwardNotifierServer.py";

        private static readonly string PythonURL = "https://www.python.org/ftp/python/3.8.3/python-3.8.3.exe";

        private static readonly string PythonArgs = "/passive InstallLauncherAllUsers=0 PrependPath=1 Include_pip=1 Include_test=0";

        private static readonly string DestFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\PythonSetup.exe";

        private static readonly string StartupFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.Startup)}";

        private static int Percentage = -1;

        private static bool IsBusy = false;

        private static bool FinishedDownloading = false;

        public static bool IsPythonInstalled()
        {
            Console.WriteLine("Checking if python is installed.");

            if (!Directory.Exists(PythonFolder))
                return false;

            string[] dirs = Directory.GetDirectories(PythonFolder);

            foreach (var i in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(i);
                if (di.Name.StartsWith("Python"))
                {
                    return true;
                }
            }

            return false;
        }

        public static void GetPython()
        {
            Console.WriteLine("Downloading python. . .");
            Console.CursorVisible = false;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                wc.DownloadFileAsync(new Uri(PythonURL), DestFile);
                
                while (!FinishedDownloading)
                    Thread.Sleep(1000);
            }
        }

        public static void InstallPython()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = DestFile,
                Arguments = PythonArgs
            };

            Process p = Process.Start(psi);
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Console.WriteLine("It appears as Python was unable to install successfully, would you like to continue with the setup anyways? Press Y for yes, else any other key for no.(Some parts of setup might crash and not work)");
                var key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Y)
                {
                    Environment.Exit(-1);
                }    
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("[##########]");
                Console.CursorVisible = true;
                Console.Write("\n");
            }
        }

        public static void GetModules()
        {
            Console.WriteLine("Getting modules. . .");

            string pythonDir = "";

            string[] dirs = Directory.GetDirectories(PythonFolder);

            foreach (var i in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(i);
                if (di.Name.StartsWith("Python"))
                {
                    PythonFolder = di.FullName;
                    pythonDir = di.FullName;
                    break;
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = $"{pythonDir}\\Scripts\\pip.exe",
                Arguments = PipCommand,
                UseShellExecute = false
            };

            Process p = Process.Start(psi);
            p.WaitForExit();
        }

        public static void GetInstallServer()
        {
            Console.WriteLine("Getting server.");

            if (File.Exists($"{StartupFolder}\\Server.pyw"))
                File.Delete($"{StartupFolder}\\Server.pyw");

            WebClient wc = new WebClient();

            string file = wc.DownloadString(ServerURL);

            if (Directory.Exists(Path.Combine(ProgramsFolder, "ForwardNotifier")))
                Directory.Delete(Path.Combine(ProgramsFolder, "ForwardNotifier"), true);

            Directory.CreateDirectory($"{ProgramsFolder}\\ForwardNotifier");

            FileStream server = File.Create($"{ProgramsFolder}\\ForwardNotifier\\Server.pyw");
            server.Write(Encoding.ASCII.GetBytes(file), 0, Encoding.ASCII.GetBytes(file).Length);
            server.Close();

            using (StreamWriter sw = new StreamWriter($"{StartupFolder}\\ForwardNotifier.bat"))
            {
                sw.WriteLine($"start pythonw \"{ProgramsFolder}\\ForwardNotifier\\Server.pyw\"");
            }

            wc.Dispose();

            Console.WriteLine("Done! The server should be starting now, if the server doesn't work, restart or go to " + StartupFolder + " and double click on ForwardNotifier.bat to launch the server!");

            Process.Start($"{StartupFolder}\\ForwardNotifier.bat");
        }

        public static void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            FinishedDownloading = true;
        }

        public static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int current = e.ProgressPercentage.Round() / 10;

            if ((current != Percentage || Percentage == -1) && !IsBusy)
            {
                IsBusy = true;
                Percentage = current;

                StringBuilder Hashes = new StringBuilder("[          ]");

                for (int i = 1; i < Percentage - 1; i++)
                {
                    Hashes[i] = '#';
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(Hashes.ToString());
            }
            Console.SetCursorPosition(12, Console.CursorTop);
            Console.Write("            ");
            IsBusy = false;
        }
    }

    public static class Extensions
    {
        public static int Round(this int input) => ((int)Math.Round(input / 10.00)) * 10;
    }

}
