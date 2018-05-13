using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Cirilla.Modules
{
    public class Link : ModuleBase
    {
        [Command("links")]
        [Summary("Displays all saved links")]
        public async Task GetLinks()
        {
            try
            {
                if (Context.User is IGuildUser guilduser)
                {
                    if (!guilduser.GuildPermissions.Administrator)
                    {
                        await ReplyAsync("You're not allowed to add links! Ask an Admin!");
                        return;
                    }
                } else
                {
                    await ReplyAsync("Links can only be used on a guild!");
                    return;
                }

                string links = GetLinks(Context.Guild.Id);

                if (!string.IsNullOrWhiteSpace(links))
                {
                    var builder = new EmbedBuilder
                    {
                        Color = new Color(66, 134, 244)
                    };

                    builder.AddField(":link: Links:", links);
                    await ReplyAsync("", embed: builder.Build());
                } else
                {
                    await ReplyAsync(
                        "No Links are saved yet! Start adding links with `$addlink \"Name\" \"http://url.com\"`!");
                }

                ConsoleHelper.Log($"{Context.User} requested links.", LogSeverity.Info);
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not get links! ({ex.Message})", LogSeverity.Error);
                await ReplyAsync("Whoops, can't show you my links right now.. :confused:");
            }
        }

        [Command("addlink")]
        [Summary("Saves a new link")]
        public async Task AddLinkCommand(
            [Summary("The name of the Link (Use \"<name>\" when the name has whitespaces")]
            string name,
            [Summary("The link to store")] string link)
        {
            try
            {
                if (Context.User is IGuildUser guilduser)
                {
                    if (!guilduser.GuildPermissions.Administrator)
                    {
                        await ReplyAsync("You're not allowed to add links! Ask an Admin!");
                        return;
                    }
                } else
                {
                    await ReplyAsync("Links can only be used on a guild!");
                    return;
                }

                AddLink(Context.Guild.Id, name, link);
                ConsoleHelper.Log($"{Context.User} added a new link (\"{link}\")", LogSeverity.Info);
                await ReplyAsync("Link added! :link:");
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not save link! {ex.Message}", LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't save that link.. :confused:");
            }
        }


        [Command("removelink")]
        [Summary("Removes a link")]
        public async Task RemoveLinkCommand(
            [Summary("The name of the Link (Use \"<name>\" when the name has whitespaces")]
            string name)
        {
            try
            {
                if (Context.User is IGuildUser guilduser)
                {
                    if (!guilduser.GuildPermissions.Administrator)
                    {
                        await ReplyAsync("You're not allowed to add links! Ask an Admin!");
                        return;
                    }
                } else
                {
                    await ReplyAsync("Links can only be used on a guild!");
                    return;
                }

                bool result = RemoveLink(Context.Guild.Id, name);
                if (result)
                {
                    ConsoleHelper.Log($"{Context.User} removed the \"{name}\" link", LogSeverity.Info);
                    await ReplyAsync("Link removed! :link:");
                } else
                {
                    await ReplyAsync($"I couldn't find the link \"{name}\"!");
                }
            } catch (Exception ex)
            {
                ConsoleHelper.Log($"Could not remove link! {ex.Message}", LogSeverity.Error);
                await ReplyAsync("Whoops, couldn't remove that link.. :confused:");
            }
        }


        //public LinkFile GuildLinks { get; set; } = new LinkFile();

        public string GetLinks(ulong guildId)
        {
            string file = Helper.GetPath(guildId, "links.txt");

            if (File.Exists(file))
            {
                string serialized = File.ReadAllText(file);
                var guildLinks = JsonConvert.DeserializeObject<LinkFile>(serialized);
                return guildLinks?.Links.Aggregate("",
                    (current, tuple) => current + $"[{tuple.Item1}]({tuple.Item2}){Environment.NewLine}");
            }

            return null;
        }

        public void AddLink(ulong guildId, string name, string link)
        {
            string file = Helper.GetPath(guildId, "links.txt");
            string serialized;
            LinkFile guildLinks;

            if (File.Exists(file))
            {
                serialized = File.ReadAllText(file);
                guildLinks = JsonConvert.DeserializeObject<LinkFile>(serialized) ?? new LinkFile();
            } else
            {
                guildLinks = new LinkFile();
            }

            guildLinks.Links.Add(new Tuple<string, string>(name, link));

            serialized = JsonConvert.SerializeObject(guildLinks);
            File.WriteAllText(file, serialized);
        }

        public bool RemoveLink(ulong guildId, string name)
        {
            string file = Helper.GetPath(guildId, "links.txt");
            string serialized;
            LinkFile guildLinks;

            if (File.Exists(file))
            {
                serialized = File.ReadAllText(file);
                guildLinks = JsonConvert.DeserializeObject<LinkFile>(serialized) ?? new LinkFile();
            } else
            {
                guildLinks = new LinkFile();
            }

            for (int i = 0; i < guildLinks.Links.Count; i++)
            {
                if (guildLinks.Links[i].Item1 == name)
                {
                    guildLinks.Links.RemoveAt(i);
                    serialized = JsonConvert.SerializeObject(guildLinks);
                    File.WriteAllText(file, serialized);
                    return true;
                }
            }

            return false;
        }
    }


    public class LinkFile
    {
        public List<Tuple<string, string>> Links { get; set; } = new List<Tuple<string, string>>();
    }
}