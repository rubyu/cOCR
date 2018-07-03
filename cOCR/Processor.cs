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
    using System.Net.Http;
    using Newtonsoft.Json;

    public static class Processor
    {
        public static bool IsImageExtension(string file)
        {
            var ext = Path.GetExtension(file);
            return ext == ".bmp" ||
                   ext == ".gif" ||
                   ext == ".png" ||
                   ext == ".jpeg" || ext == ".jpg" ||
                   ext == ".tiff" || ext == ".tif";
        }

        public static void BulkProcess(CLIOption.Result opt)
            => BulkProcess(opt, (d) => { });

        public static void BulkProcess(CLIOption.Result opt, Action<dynamic> postProcess)
        {
            var files = Directory.EnumerateFiles(opt.Directory, "*", SearchOption.AllDirectories);
            var index = 0;
            foreach (var file in files)
            {
                if (IsImageExtension(file))
                {
                    var fullPath = Path.GetFullPath(file);
                    Console.WriteLine($"[{index}] {fullPath}");
                    Process(opt, fullPath, postProcess);
                    index += 1;
                }
            }
        }

        public static void Process(CLIOption.Result opt, string imageFile, Action<dynamic> postProcess, int maxRetries = 10)
        {
            for (var i = 0; i < maxRetries; i++)
            {
                Console.WriteLine($"[OCR] {imageFile}");
                try
                {
                    if (ProcessImpl(opt, imageFile, postProcess))
                    {
                        Console.WriteLine($"[OK]");
                        return;
                    }
                    var waitSec = (int)Math.Pow(i + 1, 2);
                    Console.WriteLine($"[NG]");
                    Console.WriteLine($"Waiting {waitSec} sec...");
                    Task.Delay(waitSec * 1000).Wait();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
                finally
                {
                    GC.Collect();
                }
            }
        }

        private static bool ProcessImpl(CLIOption.Result opt, string imageFile, Action<dynamic> postProcess)
        {
            if (!IsImageExtension(imageFile)) return true;

            var parent = Directory.GetParent(Path.GetFullPath(imageFile)).FullName;
            var jsonFile = imageFile + ".json";
            var htmlFile = imageFile + ".html";
            var cssFile = Path.Combine(parent, "cOCR.css");
            var jsFile = Path.Combine(parent, "cOCR.js");

            if (File.Exists(jsonFile))
            {
                if (File.Exists(htmlFile)) return true;

                var jsonText = File.ReadAllText(jsonFile);
                Json2Html(imageFile, jsonFile, htmlFile, cssFile, jsFile, jsonText);

                if (File.Exists(htmlFile)) return true;

                try
                {
                    File.Delete(jsonFile);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }

            return Image2Html(opt, imageFile, jsonFile, htmlFile, cssFile, jsFile).Match(x =>
            {
                try
                {
                    x.Wait();
                    var json = x.Result;
                    postProcess(json);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
                return false;
            },
            () => false);
        }

        private static void PrepareHtmlResources(string cssFile, string jsFile)
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

        private static readonly ImageCodecInfo JpegEncoder = GetJpegEncoder();

        private static ImageCodecInfo GetJpegEncoder()
            => ImageCodecInfo.GetImageEncoders().Where(x => x.FormatID == ImageFormat.Jpeg.Guid).First();

        private static readonly IReadOnlyList<EncoderParameters> BestToWorstEncoderParameters
            = GenerateBestToWorstEncoderParameters().ToList();

        private static IEnumerable<EncoderParameters> GenerateBestToWorstEncoderParameters()
        {
            var qs = new List<int>() { 80, 75, 70, 65, 60, 55, 50, 45, 40, 35, 30, 25, 20 };
            foreach (var q in qs)
            {
                var p = new EncoderParameter(Encoder.Quality, q);
                var ps = new EncoderParameters(1);
                ps.Param[0] = p;
                yield return ps;
            }
        }

        internal static Option<byte[]> GetLossyImageBytes(string file, int maxSize = 4000000)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes.Length < maxSize)
            {
                return bytes.SomeNotNull();
            }
            foreach (var parameter in BestToWorstEncoderParameters)
            {
                using (var ms = new MemoryStream(bytes))
                using (var image = Image.FromStream(ms))
                using (var stream = new MemoryStream())
                {

                    image.Save(stream, JpegEncoder, parameter);
                    var buffer = stream.GetBuffer();
                    if (buffer.Length < maxSize)
                    {
                        return buffer.SomeNotNull();
                    }
                }
            }
            return Option.None<byte[]>();
        }

        internal static string RenderHtml(string imageFile, string json)
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

        private static void Json2Html(string imageFile, string jsonFile, string htmlFile, string cssFile, string jsFile, string jsonText)
        {
            try
            {
                var html = RenderHtml(imageFile, jsonText);
                File.WriteAllText(htmlFile, html);
                PrepareHtmlResources(cssFile, jsFile);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static string SerializeLanguageHints(IReadOnlyList<string> hints)
            => JsonConvert.SerializeObject(hints);

        private static Option<Task<dynamic>> Image2Html(
            CLIOption.Result opt, string imageFile, string jsonFile, string htmlFile, string cssFile, string jsFile)
        {
            return GetLossyImageBytes(imageFile).Map(async x =>
            {
                var languageHints = SerializeLanguageHints(opt.LanguageHints);

                using (var client = new HttpClient())
                {
                    var r = await GoogleCloudVision.QueryGoogleCloudVisionAPI(client, opt.EntryPoint, languageHints, opt.GoogleAPIKey, x);
                    Console.WriteLine($"StatusCode: {r.StatusCode}");
                    if ((int)r.StatusCode == 200)
                    {
                        var jsonText = await r.Content.ReadAsStringAsync();
                        dynamic json = JsonConvert.DeserializeObject(jsonText);
                        File.WriteAllText(jsonFile, jsonText);
                        Json2Html(imageFile, jsonFile, htmlFile, cssFile, jsFile, jsonText);
                        return json;
                    }
                    else
                    {
                        var responseText = await r.Content.ReadAsStringAsync();
                        Console.Error.WriteLine($"Response Headers: {r.Headers.ToString()}");
                        Console.Error.WriteLine($"Response Content: {responseText}");
                        return (dynamic)new Object();
                    }
                }
            });
        }
    }
}
