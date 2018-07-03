using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cOCRTests
{
    using cOCR;
    using System.IO;
    
    [TestClass()]
    public class ProcessorTests
    {
        [TestMethod()]
        public void GetLossyImageBytesTest0()
        {
            var file = @"..\..\Resources\SSDL.png";
            var image = File.ReadAllBytes(file);
            var lossy = Processor.GetLossyImageBytes(file);
            lossy.MatchSome(x =>
            {
                Assert.AreEqual(image.Length, x.Length);
            });
        }

        [TestMethod()]
        public void GetLossyImageBytesTest1()
        {
            {
                var file = @"..\..\Resources\SSDL.png";
                var image = File.ReadAllBytes(file);
                var lossy = Processor.GetLossyImageBytes(file, 2000000);
                lossy.MatchSome(x =>
                {
                    Assert.IsTrue(x.Length < 2000000);
                });
            }
            {
                var file = @"..\..\Resources\SSDL.png";
                var image = File.ReadAllBytes(file);
                var lossy = Processor.GetLossyImageBytes(file, 1000000);
                lossy.MatchSome(x =>
                {
                    Assert.IsTrue(x.Length < 1000000);
                });
            }
        }

        [TestMethod()]
        public void RenderHtmlTest()
        {
            var file = @"..\..\Resources\SSDL.png";
            var result = Processor.RenderHtml(file, "");
            Assert.AreEqual(result, $@"
<html>
<head>
<link rel=""stylesheet"" type=""text/css"" href=""cOCR.css"">
</head>
<body>
<script src=""cOCR.js""></script>
<div id=""image""><img src=""SSDL.png""></div>
<div id=""json""></div>
</body>
</html>
".Trim());
        }
    }
}
