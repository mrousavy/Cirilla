using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cirilla.Services.Pastebin {
    public static class Pastebin {
        public static async Task<string> Post(string text) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri("https://pastebin.com/api/api_post.php");
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                        { "api_dev_key",  Information.PastebinToken },
                        { "api_option", "paste" },
                        { "api_paste_code", text },
                        { "api_paste_private" , "1" },
                        { "api_paste_name", "Cirilla Bot log" },
                        { "api_paste_expire_date", "1H" }
                    };

                FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);

                HttpResponseMessage response = await client.PostAsync("https://pastebin.com/api/api_post.php", content);

                string responseString = await response.Content.ReadAsStringAsync();

                if (!responseString.StartsWith("http")) {
                    //Pastebin Error starts with "Bad .."
                    throw new HttpRequestException(responseString);
                } else {
                    //Pastebin link returned
                    return responseString;
                }
            }
        }
    }
}
