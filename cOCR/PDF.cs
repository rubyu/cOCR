/*
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
    using System.Net.Http;
    using Optional;
    using Newtonsoft.Json;
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    public static class PDF
    {
        private static BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);

        private static Image ByteArrayToImage(byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                return Image.GetInstance(System.Drawing.Image.FromStream(ms), BaseColor.WHITE);
            }
        }

        private static Rectangle VerticesToRect(float height, dynamic v)
        {
            int minX = Math.Min((int)v[0].x, (int)v[3].x);
            int maxX = Math.Max((int)v[1].x, (int)v[2].x);
            int minY = Math.Min((int)v[0].y, (int)v[1].y);
            int maxY = Math.Max((int)v[2].y, (int)v[3].y);
            return new Rectangle(minX, height - minY, maxX, height - maxY);
        }

        private static void DrawSymbol(PdfContentByte cb, float fontSize, float x, float y, string symbol, dynamic v, Document doc)
        {

            //cb.SaveState();
            //cb.SetFontAndSize(font, fontSize);
            //cb.SetTextMatrix(x, y);
            //cb.BeginText();
            //cb.ShowText(symbol);
            //cb.EndText();
            //cb.RestoreState();

            int minX = Math.Min((int)v[0].x, (int)v[3].x);
            int maxX = Math.Max((int)v[1].x, (int)v[2].x);
            int minY = Math.Min((int)v[0].y, (int)v[1].y);
            int maxY = Math.Max((int)v[2].y, (int)v[3].y);

            ColumnText ct = new ColumnText(cb);
            Phrase p = new Phrase(symbol);
            ct.SetSimpleColumn(p, minX, doc.PageSize.Height - maxY, maxX, doc.PageSize.Height - minY, 15, Element.ALIGN_LEFT);
            ct.Go();
        }

        public static byte[] GenerateFromGCV(byte[] imageBytes, string json)
        {
            dynamic d = JsonConvert.DeserializeObject(json);
            var annotation = d.responses[0].fullTextAnnotation;

            var image = ByteArrayToImage(imageBytes);

            using (var ms = new MemoryStream())
            {
                var doc = new Document();
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();
                doc.SetPageSize(new Rectangle(image.Width, image.Height));
                doc.SetMargins(0, 0, 0, 0);
                doc.NewPage();
                //doc.Add(image);
                var cb = writer.DirectContent;
 
                foreach (var page in annotation.pages)
                {
                    foreach (var block in page.blocks)
                    {
                        foreach (var paragraph in block.paragraphs)
                        {
                            //var pr = VerticesToRect(paragraph.boundingBox.vertices);
                            //Console.WriteLine($"Paragraph Rect: {pr}");
                            foreach (var word in paragraph.words)
                            {
                                foreach (var symbol in word.symbols)
                                {
                                    Rectangle sr = VerticesToRect(doc.PageSize.Height, symbol.boundingBox.vertices);
                                    var br = symbol.property.detectedBreak;
                                    var text = (string)(br == null ? symbol.text : symbol.text + " ");
                                    //Console.WriteLine($"Symbol Rect: {sr}");
                                    //Console.WriteLine(text);
                                    var fontSize = Math.Max(50, sr.Height);
                                    var x = sr.Left;
                                    var y = sr.Top;
                                    DrawSymbol(cb, fontSize, x, y, text, symbol.boundingBox.vertices, doc);
                                }
                            }
                        }
                    }
                }
             
                doc.Close();
                writer.Close();
                return ms.GetBuffer();
            }
        }
    }
}
*/