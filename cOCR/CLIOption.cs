using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace cOCR
{
    public class CLIOption
    {
        [CommandLine.Option('d', "dir",
            Required = true,
            HelpText = "Target directory.")]
        public string Directory
        {
            get; set;
        }

        [CommandLine.Option('k', "key",
            Required = true,
            HelpText = "Google API key.")]
        public string GoogleAPIKey
        {
            get; set;
        }

        [CommandLine.Option('e', "entry_point",
            DefaultValue = "https://vision.googleapis.com/v1p1beta1/images:annotate?key=",
            HelpText = "The entry point of Google Cloud Vision API.")]
        public string EntryPoint
        {
            get; set;
        }

        [CommandLine.Option('l', "language_hints",
            DefaultValue = "",
            HelpText = "Language hints. Multiple languages can be set by seperating by commas. e.g. --language_hints \"en, ja\" " +
            "Supported languages list: https://cloud.google.com/vision/docs/languages")]
        public string LanguageHints
        {
            get; set;
        }

        [CommandLine.Option('b', "bulk",
            HelpText = "Bulk converter mode.")]
        public bool Bulk
        {
            get; set;
        }

        [CommandLine.Option('c', "clipboard",
            HelpText = "Copy OCR result string to Clipboard.")]
        public bool Clipboard
        {
            get; set;
        }

        [CommandLine.Option('s', "show_result",
            HelpText = "Show OCR result html file.")]
        public bool ShowResult
        {
            get; set;
        }

        [CommandLine.Option('v', "version",
            HelpText = "Display product version.")]
        public bool Version
        {
            get; set;
        }

        [CommandLine.Option("help",
            HelpText = "Display help message.")]
        public bool Help
        {
            get; set;
        }

        [CommandLine.ParserState]
        public CommandLine.IParserState LastParserState { get; set; }

        [CommandLine.HelpOption]
        public string GetUsage()
        {
            var help = new CommandLine.Text.HelpText();
            help.AdditionalNewLineAfterOption = true;
            help.AddDashesToOption = true;

            help.AddPreOptionsLine("Usage:\r\n" +
                                   "  cocr.exe -d TARGET_DIR -k YOUR_GOOGLE_API_KEY " +
                                   "  [-l LANGUAGE_HINTS] [-b] [-c] [-s] [--help]");
            help.AddOptions(this);
            return help;
        }

        public class Result
        {
            private CLIOption CLIOption { set; get; }

            public bool ParseSuccess { internal set; get; }

            public string HelpMessage { internal set; get; }

            public Result(CLIOption cliOption, bool parseSuccess, string helpMessage)
            {
                this.CLIOption = cliOption;
                this.ParseSuccess = parseSuccess;
                this.HelpMessage = helpMessage;
            }

            public string Directory
            {
                get => CLIOption.Directory;
            }

            public string GoogleAPIKey
            {
                get => CLIOption.GoogleAPIKey;
            }

            public string EntryPoint
            {
                get => CLIOption.EntryPoint;
            }

            public IReadOnlyList<string> LanguageHints
            {
                get => CLIOption.LanguageHints.Split(',').Select(x => x.Trim()).Where(x => x.Any()).ToList();
            }

            public bool Bulk
            {
                get => CLIOption.Bulk;
            }

            public bool Clipboard
            {
                get => CLIOption.Clipboard;
            }

            public bool ShowResult
            {
                get => CLIOption.ShowResult;
            }

            public bool Version
            {
                get => CLIOption.Version;
            }

            public bool Help
            {
                get => CLIOption.Help;
            }

            public string VersionMessage
            {
                get => string.Format("{0} Version {1}", Application.ProductName, Application.ProductVersion);
            }
        }

        public static Result Parse(string[] args)
        {
            var cliOption = new CLIOption();
            var stringWriter = new StringWriter();
            var parser = new CommandLine.Parser(with => with.HelpWriter = stringWriter);

            var tryParse = parser.ParseArguments(args, cliOption);
            stringWriter.Close();
            var helpMessage = stringWriter.ToString();

            var result = new Result(cliOption, tryParse, helpMessage);
            return result;
        }

        public static Result ParseEnvironmentCommandLine()
        {
            var args = Environment.GetCommandLineArgs().ToList().Skip(1).ToArray();
            return Parse(args);
        }
    }
}
