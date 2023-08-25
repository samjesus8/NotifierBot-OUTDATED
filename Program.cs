using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;
using System.Timers;
using YouTubeBot.Commands;
using YouTubeBot.Config;
using YouTubeBot.YouTube;

namespace YouTubeBot
{
    public sealed class Program
    {
        //Discord Properties
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }

        //YouTube Properties
        private static YouTubeVideo lastRetrievedVideo = new YouTubeVideo();
        private static YouTubeEngine _YouTubeEngine = new YouTubeEngine();

        public static bool everyoneMention = false;
        public static bool notifications = true;
        public static ulong channelID = 123456789;
        static async Task Main(string[] args)
        {
            //1. Get the details of your config.json file by deserialising it
            var configJsonFile = new JSONReader();
            await configJsonFile.ReadJSON();

            //2. Setting up the Bot Configuration
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configJsonFile.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            //3. Apply this config to our DiscordClient
            Client = new DiscordClient(discordConfig);

            //5. Set up the Task Handler Ready event
            Client.Ready += Client_Ready;
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;

            //6. Set up the Commands Configuration
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJsonFile.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            //7. Register your commands
            Commands.RegisterCommands<Basic>();

            //8. Connect to get the Bot online
            await Client.ConnectAsync(SetActivity(), UserStatus.Online);

            //9. Start the YouTube notification service
            await StartYouTubeNotifier(Client, channelID);

            await Task.Delay(-1);
        }

        private static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            switch (e.Interaction.Data.CustomId)
            {
                case "everyoneButton":
                    if (everyoneMention == true)
                    {
                        everyoneMention = false;
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Disabled everyone mention"));
                        await Log("```everyoneMention = false```");
                    }
                    else
                    {
                        everyoneMention = true;
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Enabled everyone mention"));
                        await Log("```everyoneMention = true```");
                    }
                    break;

                case "notifierButton":
                    if (notifications == true)
                    {
                        notifications = false;
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Disabled Notifications"));
                        await Log("```notifications = false```");
                    }
                    else
                    {
                        notifications = true;
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Enabled Notifications"));
                        await Log("```notifications = true```");
                    }
                    break;
            }
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private static async Task StartYouTubeNotifier(DiscordClient client, ulong channelIdToNotify)
        {
            var timer = new Timer(1800000); //Default = 1800000, Testing = 10000
            DateTime? lastCheckedAt = DateTime.MinValue;

            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    YouTubeVideo latestVideo = _YouTubeEngine.GetLatestVideo(); // Get latest video using API

                    if (latestVideo != null && latestVideo.videoId != lastRetrievedVideo.videoId && latestVideo.PublishedAt > lastCheckedAt)
                    {
                        lastCheckedAt = latestVideo.PublishedAt; // Update the last checked timestamp

                        if (notifications)
                        {
                            string message = everyoneMention
                                ? $"(@everyone)\n" +
                                  $"SamJesus8 UPLOADED A NEW VIDEO, CHECK IT OUT!!!!\n" +
                                  $"Title: **{latestVideo.videoTitle}**\n" +
                                  $"Published at: **{latestVideo.PublishedAt}**\n" +
                                  $"URL: {latestVideo.videoUrl}"

                                : $"SamJesus8 UPLOADED A NEW VIDEO, CHECK IT OUT!!!!\n" +
                                  $"Title: **{latestVideo.videoTitle}**\n" +
                                  $"Published at: **{latestVideo.PublishedAt}**\n" +
                                  $"URL: {latestVideo.videoUrl}";

                            var channel = await client.GetChannelAsync(channelIdToNotify);
                            await channel.SendMessageAsync(message);

                            lastRetrievedVideo = latestVideo;
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now}] YouTube API: Notifications is not enabled, notifier did not run");
                            await Log($"[{DateTime.Now}] YouTube API: Notifications is not enabled, notifier did not run");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] YouTube API: No new videos were found");
                        await Log($"```[{DateTime.Now}] YouTube API: No new videos were found```");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] YouTube API: An error occurred \n {ex}");
                    await Log($"```[{DateTime.Now}] YouTube API: An error occurred``` \n {ex}");
                }
            };

            timer.Start();
        }

        private static DiscordActivity SetActivity()
        {
            var activity = new DiscordActivity
            {
                Name = "~subscribe"
            };

            return activity;
        }

        private static async Task Log(string message)
        {
            var commandCentre = await Client.GetGuildAsync(123456789);
            var logChannel = commandCentre.GetChannel(123456789);

            var log = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Black)
                    .WithTitle("samjesus8 Notification System")
                    .WithDescription(message)
                    .WithFooter(DateTime.Now.ToString()));

            await logChannel.SendMessageAsync(log);
        }
    }
}
