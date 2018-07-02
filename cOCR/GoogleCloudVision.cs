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
    using Optional;
    using System.IO;

    using R = System.Net.Http.HttpResponseMessage;

    public class GoogleCloudVision
    { 
        private string ToJsonRequest(string b64, string languageHints) 
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


    async public Task<Option<R, R>> QueryGoogleCloudVisionAPI(string entryPoint, string languageHints, string apiKey, byte[] imageBytes)
        {
#if DEBUG
            var r = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(@"..\..\test\SSDL.png.json"))
            };
            return Option.Some<R, R>(r);
#else
            var uri = entryPoint + apiKey;
            var b64 = Convert.ToBase64String(imageBytes);
            var jsonRequest = ToJsonRequest(b64, languageHints);

            Console.WriteLine($"EntryPoint: {entryPoint + String.Join("", apiKey.Select(x => 'X'))}");
            Console.WriteLine($"RequestContent: {ToJsonRequest("... snip ...", languageHints)}");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var r = await client.PostAsync(uri, content);
                Console.WriteLine($"StatusCode: {r.StatusCode}");
                if ((int)r.StatusCode == 200)
                {
                    return Option.Some<R, R>(r);
                }
                return Option.None<R, R>(r);
            }
#endif
        }
    }
}
