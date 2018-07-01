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

    using R = System.Net.Http.HttpResponseMessage;

    public class GoogleCloudVision
    {
        async public Task<Option<R, R>> QueryGoogleCloudVisionAPI(byte[] imageBytes)
        {
            var API_KEY = "";
            var uri = $"https://vision.googleapis.com/v1p1beta1/images:annotate?key={API_KEY}";
            var b64 = Convert.ToBase64String(imageBytes);
            var json = $@"
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
            ]
        }}
    ]
}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var r = await client.PostAsync(uri, content);
                if ((int)r.StatusCode == 200)
                {
                    return Option.Some<R, R>(r);
                }
                return Option.None<R, R>(r);
            }
        }
    }
}
