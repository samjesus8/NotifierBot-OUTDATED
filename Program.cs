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
        private static YouTubeVideo _video = new YouTubeVideo();
        private static YouTubeVideo temp = new YouTubeVideo();
        private static YouTubeEngine _YouTubeEngine = new YouTubeEngine();

        public static bool everyoneMention = false;
        public static ulong channelID = 745837589767913472;
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
                    }
                    else
                    {
                        everyoneMention = true;
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Enabled everyone mention"));
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
            var timer = new Timer(18000000); //Set to 18000000 for 30 min

            timer.Elapsed += async (sender, e) => {

                _video = _YouTubeEngine.GetLatestVideo(); //Get latest video using API
                DateTime lastCheckedAt = DateTime.Now;

                if (_video != null)
                {
                    if (temp.videoTitle == _video.videoTitle) //This ensures that only the newest videos get sent through
                    {
                        Console.WriteLine($"[{lastCheckedAt}] YouTube API: No new videos were found");
                    }
                    else if (_video.PublishedAt < lastCheckedAt) //If the new video is actually new
                    {
                        try
                        {
                            string message = string.Empty;
                            if (everyoneMention == true)
                            {
                                message = "(@everyone) \n" +
                                          "SamJesus8 UPLOADED A NEW VIDEO, CHECK IT OUT!!!! \n" +
                                          $"Title: **{_video.videoTitle}** \n" +
                                          $"Published at: **{_video.PublishedAt}** \n" +
                                          $"URL: {_video.videoUrl}";
                            }
                            else
                            {
                                message = "SamJesus8 UPLOADED A NEW VIDEO, CHECK IT OUT!!!! \n" +
                                          $"Title: **{_video.videoTitle}** \n" +
                                          $"Published at: **{_video.PublishedAt}** \n" +
                                          $"URL: {_video.videoUrl}";
                            }


                            await client.GetChannelAsync(channelIdToNotify).Result.SendMessageAsync(message);
                            temp = _video;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{lastCheckedAt}] YouTube API: An error occured \n {ex}");
                        }
                    }
                    else //NO new videos were found here
                    {
                        Console.WriteLine($"[{lastCheckedAt}] YouTube API: No new videos were found");
                    }
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
    }
}
