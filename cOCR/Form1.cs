using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cOCR
{
    using System.IO;
    using System.Drawing.Imaging;
    using Optional;
    using System.Net;
    using Newtonsoft.Json;

    using R = System.Net.Http.HttpResponseMessage;

    public partial class Form1 : Form
    {
        // Forcely make this application invisible from task switcher applications.
        const int WS_EX_TOOLWINDOW = 0x00000080;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | WS_EX_TOOLWINDOW;
                return cp;
            }
        }
        
        private readonly GoogleCloudVision gcv = new GoogleCloudVision();
        private readonly FileSystemWatcher fsWatcher = new FileSystemWatcher();
        
        public Form1(CLIOption.Result opt)
        {
            InitializeComponent();
            
            if (opt.Bulk)
            {
                Console.WriteLine("Mode: BulkProcess");
                BulkProcess(opt);
                Application.Exit();
            }
            else
            {
                Console.WriteLine(" Mode: FileSystemWatcher");
                fsWatcher.Path = opt.Directory;
                fsWatcher.Filter = "*";
                fsWatcher.NotifyFilter = NotifyFilters.FileName;
                fsWatcher.IncludeSubdirectories = true;
                fsWatcher.Created += async (Object sender, FileSystemEventArgs args) => 
                {
                    if (args.ChangeType == WatcherChangeTypes.Created)
                    {
                        Console.WriteLine($"Created: {args.FullPath}");
                        await Process(opt, args.FullPath);
                    }
                };
                fsWatcher.EnableRaisingEvents = true;
            }
        }

        private void InvokeProperly(MethodInvoker invoker)
        {
            if (InvokeRequired)
            {
                Invoke(invoker);
            }
            else
            {
                invoker.Invoke();
            }
        }

        private bool IsImageExtension(string ext)
            => ext == ".bmp" ||
               ext == ".gif" ||
               ext == ".png" ||
               ext == ".jpeg" || ext == ".jpg" ||
               ext == ".tiff" || ext == ".tif";

        async private void BulkProcess(CLIOption.Result opt)
        {
            var files = Directory.EnumerateFiles(opt.Directory, "*", SearchOption.AllDirectories);
            var index = 0;
            foreach (var file in files)
            {
                var fullPath = Path.GetFullPath(file);
                Console.WriteLine($"[{index}] {fullPath}");
                if (await Process(opt, fullPath))
                {
                    await Task.Delay(1000);
                }
                index += 1;
            }
        }

        async public Task<bool> Process(CLIOption.Result opt, string imageFile)
        {
            if (!IsImageExtension(Path.GetExtension(imageFile))) return true;

            var jsonFile = imageFile + ".json";
            var htmlFile = imageFile + ".html";
            var errorFile = imageFile + "error.txt";

            if (File.Exists(jsonFile))
            {
                if (File.Exists(htmlFile)) return true;

                var json = File.ReadAllText(jsonFile);
                Json2Html(imageFile, jsonFile, htmlFile, errorFile, json);

                if (File.Exists(htmlFile)) return true;

                try
                {
                    File.Delete(jsonFile);
                }
                catch { }
            }

            try
            {
                var r = await Image2Html(opt, imageFile, jsonFile, htmlFile, errorFile);
                r.Match(async (x) =>
                {
                    var d = await x;
                    string text = d.responses[0].fullTextAnnotation.text;
                    if (opt.Clipboard)
                    {
                        InvokeProperly(() =>
                        {
                            Clipboard.SetText(text);
                        });
                    }
                    if (opt.ShowResult)
                    {
                        System.Diagnostics.Process.Start(htmlFile);
                    }
                }, (x) =>
                {
                    File.AppendAllText(errorFile, x.ToString());
                });
                return r.HasValue;
            }
            catch (Exception ex)
            {
                File.AppendAllText(errorFile, ex.ToString());
            }
            return false;
        }

        private byte[] GetLossyImageBytes(string file)
        {
            if (file.EndsWith(".jpeg") || file.EndsWith(".jpg") || file.EndsWith(".gif"))
            {
                return File.ReadAllBytes(file);
            }
            var image = Image.FromFile(file);
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Jpeg);
                return stream.GetBuffer();
            }
        }

        private string RenderHtml(string imageFile, string json)
        {
            var fileName = Path.GetFileName(imageFile);
            var html_escaped_file = WebUtility.HtmlEncode(Path.GetFileName(fileName));
            var html_escaped_json = WebUtility.HtmlEncode(json);
            return $@"
<html>
<head>
<link rel=""stylesheet"" type=""text/css"" href=""cOCR.css"">
</head>
<body>
<script src=""cOCR.js""></script>
<div id=""image""><img src=""{html_escaped_file}""></div>
<div id=""json"">{html_escaped_json}</div>
</body>
</html>
".Trim();
        }

        private bool Json2Html(string imageFile, string jsonFile, string htmlFile, string errorFile, string jsonText)
        {
            try
            {
                var html = RenderHtml(imageFile, jsonText);
                File.WriteAllText(htmlFile, html);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(errorFile, ex.ToString());
                }
                catch { }
            }
            return false;
        }

        private string SerializeLanguageHints(IReadOnlyList<string> hints)
            => JsonConvert.SerializeObject(hints);


        async private Task<Option<Task<dynamic>, R>> Image2Html(CLIOption.Result opt, string imageFile, string jsonFile, string htmlFile, string errorFile)
        {
            var imageBytes = GetLossyImageBytes(imageFile);
            var languageHints = SerializeLanguageHints(opt.LanguageHints);

            var r = await gcv.QueryGoogleCloudVisionAPI(opt.EntryPoint, languageHints, opt.GoogleAPIKey, imageBytes);
            return r.Map(async (x) => {
                var jsonText = await x.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(jsonText);
                File.WriteAllText(jsonFile, jsonText);
                Json2Html(imageFile, jsonFile, htmlFile, errorFile, jsonText);
                return json;
            });
        }
    }
}
