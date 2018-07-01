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
    using System.Net;
    using Newtonsoft.Json;


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

        public Form1()
        {
            InitializeComponent();

            // todo arugment
            // -d target directory
            // -k google api key


            // -w --watch enable_target_directory watching on startup
            // -s --scan_existing_files // forcely ? should not to be implemented ?

            // -e entry_point

            // -l language

            // -w watcher mode // default
            // -b bulk converter mode

            // -c clipboard instead of opening html file, copy OCR result to clipboard

            // -m mode 

            // cilck to open the latest result
                // より、explorerで監視フォルダを開く、かな

            Image2Html(@"..\..\test\SSDL.png");

            //var file = @"..\..\test\SSDL.png";
            //var json = File.ReadAllText(@"..\..\test\SSDL.png.json");
            //var imageBytes = GetLossyImageBytes(file);
            //var pdf = PDF.GenerateFromGCV(imageBytes, json);
            //File.WriteAllBytes(@"..\..\test\SSDL.png.pdf", pdf);
            //System.Diagnostics.Process.Start(@"..\..\test\SSDL.png.pdf");
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


        private string RenderHtml(string file, string desc, string json)
        {
            return $@"
<html>
<head>
<link rel=""stylesheet"" type=""text/css"" href=""cOCR.css"">
</head>
<body>
<script src=""cOCR.js""></script>
<div id=""image""><img src=""{WebUtility.HtmlEncode(Path.GetFileName(file))}""></div>
<div id=""json"">{WebUtility.HtmlEncode(json).Replace("\r\n", "\n").Replace("\n", "<br>")}</div>
</body>
</html>
".Trim();
        }

        private void Json2Html(string file, string json)
        {
            var dir = Directory.GetParent(file).FullName;
            var filebase = Path.GetFileName(file);
            var output_html = Path.Combine(dir, $"{filebase}.html");
            var output_error = Path.Combine(dir, $"{filebase}.error.txt");

            try
            {
                dynamic d = JsonConvert.DeserializeObject(json);
                var desc = d.responses[0].textAnnotations[0].description.Value;
                var html = RenderHtml(file, desc, json);
                File.WriteAllText(output_html, html);
                System.Diagnostics.Process.Start(output_html);
            }
            catch (Exception ex)
            {
                File.AppendAllText(output_error, ex.ToString());
            }
        }

        async private void Image2Html(string file)
        {
            var dir = Directory.GetParent(file).FullName;
            var filebase = Path.GetFileName(file);
            var output_json = Path.Combine(dir, $"{filebase}.json");
            var output_error = Path.Combine(dir, $"{filebase}.error.txt");

            var imageBytes = GetLossyImageBytes(file);

            var r = await gcv.QueryGoogleCloudVisionAPI(imageBytes);
            r.Match(async (x) =>
            {
                var json = await x.Content.ReadAsStringAsync();
                File.WriteAllText(output_json, json);
                Json2Html(file, json);
            }, (x) =>
            {
                File.AppendAllText(output_error, x.ToString());
            });
        }
    }
}
