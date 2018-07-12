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
        private readonly FileSystemWatcher fsWatcher = new FileSystemWatcher();
        
        public Form1(CLIOption.Result opt)
        {
            this.cliOption = opt;

            InitializeComponent();
            
            if (opt.Bulk)
            {
                throw new InvalidOperationException();
            }

            Console.WriteLine("Mode: FileSystemWatcher");

            fsWatcher.Path = opt.Directory;
            fsWatcher.Filter = "*";
            fsWatcher.NotifyFilter = NotifyFilters.FileName;
            fsWatcher.IncludeSubdirectories = true;
            fsWatcher.Created += (Object sender, FileSystemEventArgs args) =>
            {
                if (args.ChangeType == WatcherChangeTypes.Created)
                {
                    if (Processor.IsImageExtension(args.FullPath))
                    {
                        Console.WriteLine($"[Created] {args.FullPath}");
                        Processor.Process(opt, args.FullPath, (json) => 
                        {
                            string text = json.responses[0].fullTextAnnotation.text;
                            if (opt.Clipboard)
                            {
                                InvokeProperly(() =>
                                {
                                    try
                                    {
                                        ClipboardHelper.SetText(text);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Error.WriteLine(ex.ToString());
                                    }
                                });
                            }
                            if (opt.ShowResult)
                            {
                                try
                                {
                                    System.Diagnostics.Process.Start(args.FullPath + ".html");
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex.ToString());
                                }
                            }
                            if (opt.Notice)
                            {
                                InvokeProperly(() =>
                                {
                                    ShowBalloon(text, "200 OK", ToolTipIcon.Info, 5);
                                });
                            }
                        });
                    }
                }
            };
            fsWatcher.EnableRaisingEvents = true;
        }

        private void SetTaskTrayIconTitle(string title)
        {
            Console.WriteLine($"TaskTrayIconTitle: {title}");
            notifyIcon1.Text = title;
        }

        private void ShowBalloon(string text, string title, ToolTipIcon icon, int timeout)
        {
            Console.WriteLine($"BalloonTipText: {text}");
            Console.WriteLine($"BalloonTipTitle: {title}");
            Console.WriteLine($"BalloonTipIcon: {icon}");
            Console.WriteLine($"BalloonTimeout: {timeout}");
            InvokeProperly(delegate ()
            {
                notifyIcon1.BalloonTipText = text;
                notifyIcon1.BalloonTipTitle = title;
                notifyIcon1.BalloonTipIcon = icon;
                notifyIcon1.ShowBalloonTip(timeout);
            });
        }

        protected override void OnShown(EventArgs e)
        {
            SetTaskTrayIconTitle($"cOCR - {cliOption.Directory}");
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
        
        private void NotifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Reflection.MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                method.Invoke(notifyIcon1, null);
            }
        }

        private void ClearContextMenu() => contextMenuStrip1.Items.Clear();

        private void ResetContextMenu()
        {
            ClearContextMenu();
            {
                var item = new ToolStripMenuItem($"cOCR {Application.ProductVersion}");
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripSeparator();
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem($"Dir: {cliOption.Directory}");
                item.Click += (_s, _e) =>
                {
                    System.Diagnostics.Process.Start("cliOption.Directory");
                };
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem($"EntryPoint: {cliOption.EntryPoint}");
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripSeparator();
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem($"Copy to Clipboard");
                item.Checked = cliOption.Clipboard;
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem($"Show Result");
                item.Checked = cliOption.ShowResult;
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem($"Notification");
                item.Checked = cliOption.Notice;
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripSeparator();
                contextMenuStrip1.Items.Add(item);
            }
            {
                var item = new ToolStripMenuItem("Exit");
                item.Click += (_s, _e) =>
                {
                    Close();
                    Application.ExitThread();
                };
                contextMenuStrip1.Items.Add(item);
            }
        }

        private void ContextMenu1_Opening(object sender, CancelEventArgs e) => ResetContextMenu();
    }
}
