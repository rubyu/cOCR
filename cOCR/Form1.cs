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

        private readonly CLIOption.Result cliOption;
        private readonly GoogleCloudVision gcv = new GoogleCloudVision();
        private readonly FileSystemWatcher fsWatcher = new FileSystemWatcher();
        
        public Form1(CLIOption.Result opt)
        {
            this.cliOption = opt;

            InitializeComponent();
            
            if (opt.Bulk)
            {
                Console.WriteLine("Mode: BulkConverter");

                Task.Factory.StartNew(() => 
                {
                    BulkProcess(opt);
                }).ContinueWith(x =>
                {
                    InvokeProperly(() => 
                    {
                        Close();
                    });
                    Application.ExitThread();
                });
                
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
                        if (IsImageExtension(args.FullPath))
                        {
                            Console.WriteLine($"Created: {args.FullPath}");
                            await Process(opt, args.FullPath);
                        }
                    }
                };
                fsWatcher.EnableRaisingEvents = true;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            notifyIcon1.Text = $"oOCR - {cliOption.Directory}";
            RegisterNotifyIcon(notifyIcon1);
            base.OnShown(e);
        }

        protected void RegisterNotifyIcon(NotifyIcon notifyIcon)
        {
            while (true)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                notifyIcon.Visible = true;
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds < 4000)
                {
                    break;
                }
                notifyIcon.Visible = false;
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

        private bool IsImageExtension(string file)
        {
            var ext = Path.GetExtension(file);
            return ext == ".bmp" ||
                   ext == ".gif" ||
                   ext == ".png" ||
                   ext == ".jpeg" || ext == ".jpg" ||
                   ext == ".tiff" || ext == ".tif";
        }

        private void BulkProcess(CLIOption.Result opt)
        {
            var files = Directory.EnumerateFiles(opt.Directory, "*", SearchOption.AllDirectories);
            var index = 0;
            foreach (var file in files)
            {
                if (IsImageExtension(file))
                {
                    var fullPath = Path.GetFullPath(file);
                    Console.WriteLine($"[{index}] {fullPath}");
                    var task = Process(opt, fullPath);
                    task.Wait();
                    index += 1;
                }
            }
        }

        async public Task<bool> Process(CLIOption.Result opt, string imageFile)
        {
            if (!IsImageExtension(imageFile)) return true;

            var parent = Directory.GetParent(Path.GetFullPath(imageFile)).FullName;
            var jsonFile = imageFile + ".json";
            var htmlFile = imageFile + ".html";
            var cssFile = Path.Combine(parent, "cOCR.css");
            var jsFile = Path.Combine(parent, "cOCR.js");
            var errorFile = imageFile + "error.txt";

            if (File.Exists(jsonFile))
            {
                if (File.Exists(htmlFile)) return true;

                var jsonText = File.ReadAllText(jsonFile);
                Json2Html(imageFile, jsonFile, htmlFile, cssFile, jsFile, errorFile, jsonText);

                if (File.Exists(htmlFile)) return true;
                
                try
                {
                    File.Delete(jsonFile);
                }
                catch { }
            }

            try
            {
                var r = await Image2Html(opt, imageFile, jsonFile, htmlFile, cssFile, jsFile, errorFile);
                r.Match(async (x) =>
                {
                    var d = await x;
                    string text = d.responses[0].fullTextAnnotation.text;
                    if (!opt.Bulk && opt.Clipboard)
                    {
                        InvokeProperly(() =>
                        {
                            Clipboard.SetText(text);
                        });
                    }
                    if (!opt.Bulk && opt.ShowResult)
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

        private void PrepareHtmlResources(string cssFile, string jsFile)
        {
            try
            {
                if (!File.Exists(cssFile))
                {
                    File.WriteAllText(cssFile, Properties.Resources.DefaultCSS);
                }
            }
            catch { }
            try
            {
                if (!File.Exists(jsFile))
                {
                    File.WriteAllText(jsFile, Properties.Resources.DefaultJS);
                }
            }
            catch { }
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

        private bool Json2Html(string imageFile, string jsonFile, string htmlFile, string cssFile, string jsFile, string errorFile, string jsonText)
        {
            try
            {
                var html = RenderHtml(imageFile, jsonText);
                File.WriteAllText(htmlFile, html);
                PrepareHtmlResources(cssFile, jsFile);
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

        async private Task<Option<Task<dynamic>, R>> Image2Html(
            CLIOption.Result opt, string imageFile, string jsonFile, string htmlFile, string cssFile, string jsFile, string errorFile)
        {
            var imageBytes = GetLossyImageBytes(imageFile);
            var languageHints = SerializeLanguageHints(opt.LanguageHints);

            var r = await gcv.QueryGoogleCloudVisionAPI(opt.EntryPoint, languageHints, opt.GoogleAPIKey, imageBytes);
            return r.Map(async (x) => {
                var jsonText = await x.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(jsonText);
                File.WriteAllText(jsonFile, jsonText);
                Json2Html(imageFile, jsonFile, htmlFile, cssFile, jsFile, errorFile, jsonText);
                return json;
            });
        }

        private void NotifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Reflection.MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(notifyIcon1, null);
            }
        }

        private void ClearContextMenu()
        {
            contextMenuStrip1.Items.Clear();
        }

        private void RegisterContextMenuItems0()
        {
            var item = new ToolStripMenuItem($"cOCR {Application.ProductVersion}");
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems1()
        {
            var item = new ToolStripSeparator();
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems2()
        {
            var item = new ToolStripMenuItem($"Dir: {cliOption.Directory}");
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems3()
        {
            var item = new ToolStripMenuItem($"EntryPoint: {cliOption.EntryPoint}");
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems4()
        {
            var item = new ToolStripSeparator();
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems5()
        {
            var item = new ToolStripMenuItem($"Bulk Converter");
            item.Checked = cliOption.Bulk;
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems6()
        {
            var item = new ToolStripMenuItem($"File System Watcher");
            item.Checked = !cliOption.Bulk;
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems7()
        {
            var item = new ToolStripSeparator();
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems8()
        {
            var item = new ToolStripMenuItem($"Copy to Clipboard");
            item.Checked = cliOption.Clipboard;
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems9()
        {
            var item = new ToolStripMenuItem($"Show Result");
            item.Checked = cliOption.ShowResult;
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems10()
        {
            var item = new ToolStripSeparator();
            contextMenuStrip1.Items.Add(item);
        }

        private void RegisterContextMenuItems11()
        {
            var item = new ToolStripMenuItem("Exit");
            item.Click += (_s, _e) =>
            {
                Close();
                Application.ExitThread();
            };
            contextMenuStrip1.Items.Add(item);
        }

        private void ResetContextMenu()
        {
            ClearContextMenu();
            RegisterContextMenuItems0();
            RegisterContextMenuItems1();
            RegisterContextMenuItems2();
            RegisterContextMenuItems3();
            RegisterContextMenuItems4();
            RegisterContextMenuItems5();
            RegisterContextMenuItems6();
            RegisterContextMenuItems7();
            RegisterContextMenuItems8();
            RegisterContextMenuItems9();
            RegisterContextMenuItems10();
            RegisterContextMenuItems11();
        }

        private void ContextMenu1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResetContextMenu();
        }
    }
}
