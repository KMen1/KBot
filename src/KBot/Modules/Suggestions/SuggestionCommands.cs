using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace KBot.Modules.Suggestions;

[DefaultMemberPermissions(GuildPermission.SendMessages)]
public class SuggestionCommands : SlashModuleBase
{
    [SlashCommand("suggest", "Create a new suggestion")]
    public async Task CreateSuggestionAsync(string title, string description)
    {
        await DeferAsync().ConfigureAwait(false);
        var config = await Mongo.GetGuildConfigAsync(Context.Guild).ConfigureAwait(false);
        if (config.SuggestionChannelId == 0)
        {
            var eb = new EmbedBuilder()
                .WithDescription(
                    "**Suggestions are disabled on this server! Please ask an admin to enable them!**"
                )
                .WithColor(Color.Red)
                .Build();
            await FollowupAsync(embed: eb, ephemeral: true).ConfigureAwait(false);
            return;
        }

        var embed = new EmbedBuilder()
            .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(Color.Blue)
            .Build();
        var comp = new ComponentBuilder()
            .WithButton(
                "Accept",
                $"suggest-accept:{Context.User.Id}",
                ButtonStyle.Success,
                new Emoji("✅")
            )
            .WithButton(
                "Deny",
                $"suggest-decline:{Context.User.Id}",
                ButtonStyle.Danger,
                new Emoji("❌")
            )
            .Build();

        var suggestionChannel = Context.Guild.GetTextChannel(config.SuggestionChannelId);
        await suggestionChannel
            .SendMessageAsync(embed: embed, components: comp)
            .ConfigureAwait(false);
        await FollowupWithEmbedAsync(
                Color.Green,
                "Suggestion Created",
                $"In Channel: {suggestionChannel.Mention}"
            )
            .ConfigureAwait(false);
    }
}
