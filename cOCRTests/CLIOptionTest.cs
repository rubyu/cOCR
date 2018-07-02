using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cOCRTests
{
    using cOCR;

    [TestClass()]
    public class CLIOptionTests
    {
        [TestMethod()]
        public void ParseDirectoryTest()
        {
            {
                string[] args = { "-d" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                string[] args = { "--dir" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                string dir = "dummy";
                string[] args = { "-d", dir, "-k", "key" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.Directory, dir);
            }
            {
                string dir = "dummy";
                string[] args = { "-d", dir, "--key", "key" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.Directory, dir);
            }
            {
                string dir = "dummy";
                string[] args = { "--dir", dir, "-k", "key" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.Directory, dir);
            }
            {
                string dir = "dummy";
                string[] args = { "--dir", dir, "--key", "key" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.Directory, dir);
            }
        }

        [TestMethod()]
        public void ParseKeyTest()
        {
            {
                string[] args = { "-k" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                string[] args = { "--key" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                string key = "dummy";
                string[] args = { "-d", "dir", "-k", key };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.GoogleAPIKey, key);
            }
            {
                string key = "dummy";
                string[] args = { "--dir", "dir", "-k", key };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.GoogleAPIKey, key);
            }
            {
                string key = "dummy";
                string[] args = { "-d", "dir", "--key", key };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.GoogleAPIKey, key);
            }
            {
                string key = "dummy";
                string[] args = { "--dir", "dir", "--key", key };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, true);
                Assert.AreEqual(result.GoogleAPIKey, key);
            }
        }

        [TestMethod()]
        public void ParseEntryPointTest()
        {
            {
                string[] args = { "-e" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.EntryPoint, "https://vision.googleapis.com/v1p1beta1/images:annotate?key=");
            }
            {
                string[] args = { "--entry_point" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.EntryPoint, "https://vision.googleapis.com/v1p1beta1/images:annotate?key=");
            }
            {
                var ep = "dummy";
                string[] args = { "-e", ep };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.EntryPoint, ep);
            }
            {
                var ep = "dummy";
                string[] args = { "--entry_point", ep };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.EntryPoint, ep);
            }
        }


        [TestMethod()]
        public void ParseLanguageHintsTest()
        {
            {
                string[] args = { "-l" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                string[] args = { "--language_hints" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ParseSuccess, false);
            }
            {
                var hints = "dummy";
                string[] args = { "-l", hints };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.LanguageHints.Count, 1);
                Assert.AreEqual(result.LanguageHints[0], hints);
            }
            {
                var hints = "dummy";
                string[] args = { "--language_hints", hints };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.LanguageHints.Count, 1);
                Assert.AreEqual(result.LanguageHints[0], hints);
            }
        }

        [TestMethod()]
        public void ParseBulkTest()
        {
            {
                string[] args = { };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Bulk, false);
            }
            {
                string[] args = { "-b" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Bulk, true);
            }
            {
                string[] args = { "--bulk" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Bulk, true);
            }
        }

        [TestMethod()]
        public void ParseClipboardTest()
        {
            {
                string[] args = { };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Clipboard, false);
            }
            {
                string[] args = { "-c" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Clipboard, true);
            }
            {
                string[] args = { "--clipboard" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Clipboard, true);
            }
        }

        [TestMethod()]
        public void ShowResultTest()
        {
            {
                string[] args = { };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ShowResult, false);
            }
            {
                string[] args = { "-s" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ShowResult, true);
            }
            {
                string[] args = { "--show_result" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.ShowResult, true);
            }
        }

        [TestMethod()]
        public void ParseVersionTest()
        {
            {
                string[] args = { };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Version, false);
            }
            {
                string[] args = { "-v" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Version, true);
            }
            {
                string[] args = { "--version" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Version, true);
            }
        }

        [TestMethod()]
        public void ParseHelpTest()
        {
            {
                string[] args = { };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Help, false);
            }
            {
                string[] args = { "-h" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Help, true);
            }
            {
                string[] args = { "--help" };
                var result = CLIOption.Parse(args);
                Assert.AreEqual(result.Help, true);
            }
        }
    }
}
