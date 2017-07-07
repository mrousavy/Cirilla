﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cirilla.Services.Pastebin {
    public static class Pastebin {
        public static async Task<string> Post(string text) {
            text = CheckLength(text);

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

        //Check if string is longer than 2000 chars, if yes - limit it
        public static string CheckLength(string input) {
            const int maxLength = 65520;

            if (input.Length >= maxLength) { //max length for the FormUrlEncodedContent's parameters
                return input.Substring(input.Length - maxLength, maxLength); // trim to the last 65520 chars
            }
            return input;
        }
    }
}
