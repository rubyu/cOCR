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
                var opt = CLIOption.Parse(args);
#else
                var opt = CLIOption.ParseEnvironmentCommandLine();
#endif

                if (opt.Version)
                {
                    Console.WriteLine(opt.VersionMessage);
                    Environment.Exit(0);
                }
                else if (opt.Help)
                {
                    Console.WriteLine(opt.HelpMessage);
                    Environment.Exit(0);
                }
                else if (!opt.ParseSuccess)
                {
                    Console.WriteLine(opt.HelpMessage);
                    if (opt.Directory == null)
                    {
                        MessageBox.Show("-d TARGET_DIRECTORY is required.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        Environment.Exit(1);
                    }
                    else if (opt.GoogleAPIKey == null)
                    {
                        MessageBox.Show("-k YOUR_GOOGLE_API_KEY is required.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    Environment.Exit(1);
                }

                Console.WriteLine("----------");
                Console.WriteLine($"CLIOption.Directory: {opt.Directory}");
                Console.WriteLine($"CLIOption.GoogleAPIKey: {String.Join("", opt.GoogleAPIKey.Select(x => 'X'))}");
                Console.WriteLine($"CLIOption.EntryPoint: {opt.EntryPoint}");
                Console.WriteLine($"CLIOption.LanguageHints: {String.Join(", ", opt.LanguageHints)}");
                Console.WriteLine($"CLIOption.Bulk: {opt.Bulk}");
                Console.WriteLine($"CLIOption.Clipboard: {opt.Clipboard}");
                Console.WriteLine($"CLIOption.ShowResult: {opt.ShowResult}");
                Console.WriteLine("----------");

                Application.Run(new Form1(opt));
            }
            finally
            {
                WinAPI.Console.Console.FreeConsole();
            }
        }
    }
}
