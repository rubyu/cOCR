using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cOCR
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                WinAPI.Console.Console.AttachConsole();
#if DEBUG
                string[] args = { "-d", @"..\..\test", "-k", "dummy" };
                var cliOption = CLIOption.Parse(args);
#else
                var cliOption = CLIOption.ParseEnvironmentCommandLine();
#endif

                if (cliOption.Version)
                {
                    Console.WriteLine(cliOption.VersionMessage);
                    Environment.Exit(0);
                }
                else if (cliOption.Help)
                {
                    Console.WriteLine(cliOption.HelpMessage);
                    Environment.Exit(0);
                }
                else if (!cliOption.ParseSuccess)
                {
                    Console.WriteLine(cliOption.HelpMessage);
                    if (cliOption.Directory == null)
                    {
                        MessageBox.Show("-d TARGET_DIRECTORY is required.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Environment.Exit(1);
                    }
                    else if (cliOption.GoogleAPIKey == null)
                    {
                        MessageBox.Show("-k YOUR_GOOGLE_API_KEY is required.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    Environment.Exit(1);
                }

                Console.WriteLine("----------");
                Console.WriteLine($"CLIOption.Directory: {cliOption.Directory}");
                Console.WriteLine($"CLIOption.GoogleAPIKey: {String.Join("", cliOption.GoogleAPIKey.Select(x => 'X'))}");
                Console.WriteLine($"CLIOption.EntryPoint: {cliOption.EntryPoint}");
                Console.WriteLine($"CLIOption.LanguageHints: {String.Join(", ", cliOption.LanguageHints)}");
                Console.WriteLine($"CLIOption.Bulk: {cliOption.Bulk}");
                Console.WriteLine($"CLIOption.Clipboard: {cliOption.Clipboard}");
                Console.WriteLine($"CLIOption.ShowResult: {cliOption.ShowResult}");
                Console.WriteLine("----------");

                Application.Run(new Form1(cliOption));
            }
            finally
            {
                WinAPI.Console.Console.FreeConsole();
            }
        }
    }
}
