﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KBot.Models;
using KBot.Services;

namespace KBot.Modules;

public abstract class SlashModuleBase : InteractionModuleBase<SocketInteractionContext>
{
    public MongoService Mongo { get; set; }

    protected async Task<IUserMessage> FollowupWithEmbedAsync(Color color, string title, string description,
        string url = null, string imageUrl = null, bool ephemeral = false)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithUrl(url)
            .WithImageUrl(imageUrl)
            .WithColor(color)
            .Build();
        return await Context.Interaction.FollowupAsync(embed: embed, ephemeral: ephemeral).ConfigureAwait(false);
    }

    protected Task<GuildConfig> GetGuildConfigAsync()
    {
        return Mongo.GetGuildConfigAsync(Context.Guild);
    }
}