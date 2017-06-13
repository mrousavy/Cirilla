using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using System;
using System.Threading.Tasks;

namespace Cirilla.Modules {
    public class Translation : ModuleBase {
        [Command("translate"), Summary("Translate something!")]
        public async Task Translate(
            [Summary("The language you want to translate into (Two letter code, e.g. 'de')")]string language,
            [Summary("The Google search query")] params string[] query) {
            try {
                string text = string.Join(" ", query);
                string result = "";

                string key = "AIzaSyBRRfFKm4Fb7uZdX-I7NXqRA5gLRmfjd4Y";
                TranslateService service = new TranslateService(
                    new BaseClientService.Initializer() {
                        ApiKey = key,
                        ApplicationName = "Cirilla"
                    });
                Google.Apis.Translate.v2.Data.TranslationsListResponse response = await service.Translations.List(query, language).ExecuteAsync();
                result = string.Join(" ", response.Translations);


                await ConsoleHelper.Log($"Translated \"{text}\" into {language}.", Discord.LogSeverity.Info);
                await ReplyAsync($"\"{text}\" in {language}: {result}");
            } catch (Exception ex) {
                await ConsoleHelper.Log($"Could not translate text with Google Translate! ({ex.Message})", Discord.LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't translate that for you.. Now you have to do it yourself! :confused:");
            }
        }
    }
}
