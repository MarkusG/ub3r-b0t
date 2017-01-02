﻿namespace UB3RB0T
{
    using Discord;
    using Discord.WebSocket;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Scripting;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using System.Reflection;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using System.IO;

    public class DiscordCommands
    {
        public Dictionary<string, Func<SocketMessage, Task>> Commands { get; private set; }
        private DiscordSocketClient client;

        private ScriptOptions scriptOptions;

        // TODO:
        // this is icky
        public DiscordCommands(DiscordSocketClient client)
        {
            this.client = client;
            this.Commands = new Dictionary<string, Func<SocketMessage, Task>>();

            this.CreateScriptOptions();

            Commands.Add("debug", async (message) =>
            {
                var serverId = (message.Channel as IGuildChannel)?.GuildId.ToString() ?? "n/a";
                await message.Channel.SendMessageAsync($"```Server ID: {serverId} | Channel ID: {message.Channel.Id} | Your ID: {message.Author.Id} | Shard ID: {message.Discord.ShardId}```");
                return;
            });

            Commands.Add("status", async (message) =>
            {
                var serversStatus = await Utilities.GetApiResponseAsync<HeartbeatData[]>(BotConfig.Instance.HeartbeatEndpoint);

                var dataSb = new StringBuilder();
                dataSb.Append("```cs\n" +
                   "type       shard   server count    users  \n");

                foreach (HeartbeatData heartbeat in serversStatus)
                {
                    var botType = heartbeat.BotType.PadRight(11);
                    var shard = heartbeat.Shard.ToString().PadLeft(4);
                    var servers = heartbeat.ServerCount.ToString().PadLeft(13);
                    var users = heartbeat.UserCount.ToString().PadLeft(8);

                    dataSb.Append($"{botType} {shard}  {servers} {users}\n");
                }

                dataSb.Append("```");

                await message.Channel.SendMessageAsync(dataSb.ToString());
            });

            Commands.Add("voice", async (message) =>
            {
                var channel = (message.Author as IGuildUser)?.VoiceChannel;
                if (channel == null)
                {
                    await message.Channel.SendMessageAsync("Join a voice channel first");
                }

                Task.Run(async () =>
                {
                    await AudioUtilities.JoinAudioAsync(channel);
                }).Forget();

                return;
            });

            Commands.Add("dvoice", async (message) =>
            {
                if (message.Channel is IGuildChannel channel)
                {
                    await AudioUtilities.LeaveAudioAsync(channel);
                }
            });

            Commands.Add("clear", async (message) =>
            {
                if (message.Channel is IDMChannel)
                {
                    return;
                }

                var guildUser = message.Author as IGuildUser;
                if (!guildUser.GuildPermissions.ManageMessages)
                {
                    await message.Channel.SendMessageAsync("you don't have permissions to clear messages, fartface");
                    return;
                }

                string[] parts = message.Content.Split(new[] { ' ' }, 3);
                if (parts.Length != 2 && parts.Length != 3)
                {
                    await message.Channel.SendMessageAsync("Usage: .clear #; Usage for user specific messages: .clear # username");
                    return;
                }

                IUser deletionUser = null;

                if (parts.Length == 3)
                {
                    if (ulong.TryParse(parts[2], out ulong userId))
                    {
                        deletionUser = await message.Channel.GetUserAsync(userId);
                    }
                    else
                    {
                        deletionUser = (await guildUser.Guild.GetUsersAsync().ConfigureAwait(false)).Find(parts[2]).FirstOrDefault();
                    }

                    if (deletionUser == null)
                    {
                        await message.Channel.SendMessageAsync("Couldn't find the specified user. Try their ID if nick matching is struggling");
                        return;
                    }
                }

                var botGuildUser = await (message.Channel as IGuildChannel).GetUserAsync(message.Discord.CurrentUser.Id);
                bool botOnly = deletionUser == botGuildUser;

                if (!botOnly && !botGuildUser.GetPermissions(message.Channel as IGuildChannel).ManageMessages)
                {
                    await message.Channel.SendMessageAsync("yeah I don't have the permissions to delete messages, buttwad.");
                    return;
                }

                if (int.TryParse(parts[1], out int count))
                {
                    // +1 for the current .clear message
                    if (deletionUser == null)
                    {
                        count = Math.Min(99, count) + 1;
                    }
                    else
                    {
                        count = Math.Min(100, count);
                    }

                    // download messages until we've hit the limit
                    var msgsToDelete = new List<IMessage>();
                    var msgsToDeleteCount = 0;
                    ulong? lastMsgId = null;
                    while (msgsToDeleteCount < count)
                    {
                        IEnumerable<IMessage> downloadedMsgs;
                        try
                        {
                            if (!lastMsgId.HasValue)
                            {
                                downloadedMsgs = await (message.Channel as ITextChannel).GetMessagesAsync(count).Flatten();
                            }
                            else
                            {
                                downloadedMsgs = await (message.Channel as ITextChannel).GetMessagesAsync(lastMsgId.Value, Direction.Before, count).Flatten();
                            }
                        }
                        catch (Exception ex)
                        {
                            downloadedMsgs = new IMessage[0];
                            Console.WriteLine(ex);
                        }

                        if (downloadedMsgs.Count() > 0)
                        {
                            lastMsgId = downloadedMsgs.Last().Id;
                            var msgs = downloadedMsgs.Where(m => deletionUser == null || m.Author?.Id == deletionUser.Id).Take(count - msgsToDeleteCount);
                            msgsToDeleteCount += msgs.Count();
                            msgsToDelete.AddRange(msgs);
                        }
                        else
                        {
                            break;
                        }
                    }

                    await (message.Channel as ITextChannel).DeleteMessagesAsync(msgsToDelete);
                }
                else
                {
                    await message.Channel.SendMessageAsync("Usage: .clear #");
                }
            });

            Commands.Add("userinfo", async (message) =>
            {
                SocketUser targetUser = message.MentionedUsers?.FirstOrDefault();

                if (targetUser == null)
                {
                    var messageParts = message.Content.Split(new[] { ' ' }, 2);

                    if (messageParts.Length == 2)
                    {
                        if (message.Channel is IGuildChannel guildChannel)
                        {
                            targetUser = (await guildChannel.Guild.GetUsersAsync()).Find(messageParts[1]).FirstOrDefault() as SocketUser;
                        }

                        if (targetUser == null)
                        {
                            await message.Channel.SendMessageAsync("User not found. Try a direct mention.");
                            return;
                        }
                    }
                    else
                    {
                        targetUser = message.Author;
                    }
                }
                
                var guildUser = targetUser as IGuildUser;

                if (guildUser == null)
                {
                    return;
                }

                var embedBuilder = new EmbedBuilder
                {
                    Title = $"UserInfo for {targetUser.Username}#{targetUser.Discriminator}",
                    ThumbnailUrl = targetUser.AvatarUrl
                };

                if (!string.IsNullOrEmpty(guildUser.Nickname))
                {
                    embedBuilder.Description = $"aka {guildUser.Nickname}";
                }

                embedBuilder.Footer = new EmbedFooterBuilder
                {
                    Text = CommandsConfig.Instance.UserInfoSnippets.Random()
                };

                embedBuilder.AddField((field) =>
                {
                    field.IsInline = true;
                    field.Name = "created";
                    field.Value = $"{targetUser.GetCreatedDate().ToString("dd MMM yyyy")} {targetUser.GetCreatedDate().ToString("hh:mm:ss tt")} UTC";
                });

                embedBuilder.AddField((field) =>
                {
                    field.IsInline = true;
                    field.Name = "joined";
                    field.Value = $"{guildUser.JoinedAt.Value.ToString("dd MMM yyyy")} {guildUser.JoinedAt.Value.ToString("hh:mm:ss tt")} UTC";
                });

                var roles = new List<string>();
                foreach (ulong roleId in guildUser.RoleIds)
                {
                    roles.Add(guildUser.Guild.Roles.First(g => g.Id == roleId).Name.TrimStart('@'));
                }

                embedBuilder.AddField((field) =>
                {
                    field.IsInline = false;
                    field.Name = "roles";
                    field.Value = string.Join(", ", roles);
                });

                embedBuilder.AddField((field) =>
                {
                    field.IsInline = true;
                    field.Name = "Id";
                    field.Value = targetUser.Id.ToString();
                });

                await message.Channel.SendMessageAsync(((char)1).ToString(), false, embedBuilder);
            });

            Commands.Add("roll", RollAsync);
            Commands.Add("d", RollAsync);

            Commands.Add("admin", async (message) =>
            {
                if (SettingsConfig.Instance.CreateEndpoint != null && message.Channel is IGuildChannel guildChannel)
                {
                    if ((message.Author as IGuildUser).GuildPermissions.ManageGuild)
                    {
                        var req = WebRequest.Create($"{SettingsConfig.Instance.CreateEndpoint}?id={guildChannel.GuildId}");
                        try
                        {
                            await req.GetResponseAsync();
                            await message.Channel.SendMessageAsync($"Manage from {SettingsConfig.Instance.ManagementEndpoint}");
                        }
                        catch (Exception ex)
                        {
                            // TODO: proper logging
                            Console.WriteLine(ex);
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("You must have manage server permisions to use that command. nice try, dungheap");
                    }
                }
            });

            Commands.Add("eval", async (message) =>
            {
                if (BotConfig.Instance.Discord.OwnerId == message.Author.Id)
                {
                    var script = message.Content.Split(new[] { ' ' }, 2)[1];
                    string result = "no result";
                    try
                    {
                        var evalResult = await CSharpScript.EvaluateAsync<object>(script, scriptOptions, globals: new ScriptHost { Message = message, Client = client });
                        result = evalResult.ToString();
                    }
                    catch (Exception ex)
                    {
                        result = ex.ToString().Substring(0, Math.Min(ex.ToString().Length, 800));
                    }

                    await message.Channel.SendMessageAsync($"``{result}``");
                }
            });
        }

        // TODO: Not discord specific, move this
        private async Task RollAsync(SocketMessage message)
        {
            string[] parts = message.Content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var rolls = new List<long>();
            var r = new Random();

            bool failed = true;

            if (parts.Length == 1)
            {
                rolls.Add(new Random().Next(1, 7));
            }
            else
            {
                // see if it's a multi roll
                var match = Regex.Match(parts[1], "(\\d+)?d(\\d+)");

                if (match.Success && match.Groups.Count == 3)
                {
                    string numDiceValue = string.IsNullOrEmpty(match.Groups[1].Value) ? "1" : match.Groups[1].Value;
                    if (int.TryParse(numDiceValue, out int numDice) && int.TryParse(match.Groups[2].Value, out int diceValue) && numDice <= 20)
                    {
                        long rollResult = 0;
                        for (var i = 0; i < numDice; i++)
                        {
                            rollResult += r.Next(1, Math.Min(diceValue + 1, int.MaxValue));
                        }

                        rolls.Add(rollResult);
                        failed = false;
                    }
                }
                else if (parts.Length == 2 && parts[1].Contains("i"))
                {
                    await message.Channel.SendMessageAsync("I uhh...I don't know how to deal with unreal dice.... my brain is wrinkled");
                }
                else if (parts.Length == 2 && parts[1] == "0")
                {
                    await message.Channel.SendMessageAsync("really? zero sides? ass.");
                }
                else if (parts.Length <= 20)
                {
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (int.TryParse(parts[i], out int roll) && roll > 0 && roll < int.MaxValue)
                        {
                            rolls.Add(r.Next(1, Math.Min(roll + 1, int.MaxValue)));
                            failed = false;
                        }
                        else
                        {
                            failed = true;
                            break;
                        }
                    }
                }
            }

            string msg = failed ? "dude at least one of those is a bogus die or you rolled more than 20 of 'em. don't fuck with me man this is serious" : $"You rolled ... {string.Join(" | ", rolls)}";
            await message.Channel.SendMessageAsync(msg);
        }

        private void CreateScriptOptions()
        {
            // mscorlib reference issues when using codeanalysis; 
            // see http://stackoverflow.com/questions/38943899/net-core-cs0012-object-is-defined-in-an-assembly-that-is-not-referenced
            var dd = typeof(object).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            var references = new List<MetadataReference>
            {   
                MetadataReference.CreateFromFile($"{coreDir.FullName}{Path.DirectorySeparatorChar}mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            };

            var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var referencedAssembly in referencedAssemblies)
            {
                var loadedAssembly = Assembly.Load(referencedAssembly);
                references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
            }

            this.scriptOptions = ScriptOptions.Default.
                AddImports("System", "System.Linq", "System.Text", "Discord", "Discord.WebSocket").
                AddReferences(references);
        }
    }

    public class ScriptHost
    {
        public SocketMessage Message { get; set; }
        public DiscordSocketClient Client { get; set; }
    }
}
