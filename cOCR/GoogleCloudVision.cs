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
    using System.Net.Http;
    using System.IO;

    public static class GoogleCloudVision
    {
        private static string ToJsonRequest(string b64, string languageHints) 
        => $@"
{{
    ""requests"": 
    [
        {{
            ""image"": 
            {{
                ""content"": ""{b64}""
            }},
            ""features"": 
            [
                {{
                    ""type"": ""DOCUMENT_TEXT_DETECTION""
                }}
            ],
            ""imageContext"": 
            {{
                ""languageHints"": {languageHints}
            }}
        }}
    ]
}}".Trim();


    async public static Task<HttpResponseMessage> QueryGoogleCloudVisionAPI(HttpClient client, string entryPoint, string languageHints, string apiKey, byte[] imageBytes)
        {
#if DEBUG
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(@"..\..\test\SSDL.png.json"))
            };
#else
            var uri = entryPoint + apiKey;
            var b64 = Convert.ToBase64String(imageBytes);
            var jsonRequest = ToJsonRequest(b64, languageHints);

            Console.WriteLine($"EntryPoint: {entryPoint + String.Join("", apiKey.Select(x => 'X'))}");
            Console.WriteLine($"RequestContent: {ToJsonRequest("... snip ...", languageHints)}");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            return await client.PostAsync(uri, content);
#endif
        }
    }
}
