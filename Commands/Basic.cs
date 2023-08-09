using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace YouTubeBot.Commands
{
    public class Basic : BaseCommandModule
    {
        [Command("subscribe")]
        public async Task Subscribe(CommandContext ctx) 
        {
            var subscribeMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("Subscribe to my channel")
                    .WithUrl("https://www.youtube.com/channel/UCMt7ZwKIAoE3tIDudviqUSA")
                    .WithImageUrl("https://media.discordapp.net/attachments/1020110665161113610/1138905786928615525/samj_corner_logo_2.png"));

            await ctx.Channel.SendMessageAsync(subscribeMessage);
        }

        [Command("settings")]
        public async Task Settings(CommandContext ctx)
        {
            if (ctx.User.Id == 572877986223751188)
            {
                var everyoneButton = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Primary, "everyoneButton", "1");

                var settingsMenu = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Black)
                        .WithTitle("Bot Settings")
                    .AddField("@everyone Mention", Program.everyoneMention.ToString()))
                    .AddComponents(everyoneButton);

                await ctx.Channel.SendMessageAsync(settingsMenu);
            }
        }
    }
}
