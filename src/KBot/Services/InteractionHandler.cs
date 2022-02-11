﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using KBot.Config;
using KBot.Modules.Audio.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KBot.Services;

public class InteractionHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly InteractionService _interactionService;

    public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService) : base(client, logger)
    {
        _provider = provider;
        _interactionService = interactionService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.InteractionCreated += HandleInteractionAsync;
        Client.GuildAvailable += ClientOnGuildAvailableAsync;
        _interactionService.SlashCommandExecuted += HandleSlashCommandResultAsync;
        _interactionService.ComponentCommandExecuted += HandleComponentCommandResultAsync;

        try
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to load modules");
        }
        await Client.WaitForReadyAsync(stoppingToken).ConfigureAwait(false);
        await Client.SetGameAsync("/" + _provider.GetRequiredService<BotConfig>().Client.Game, type: ActivityType.Listening).ConfigureAwait(false);
        await Client.SetStatusAsync(UserStatus.Online).ConfigureAwait(false);
        /*foreach (var guild in Client.Guilds)
            await _interactionService.AddModulesToGuildAsync(guild, true, _interactionService.Modules.ToArray()).ConfigureAwait(false);*/
    }

    private Task ClientOnGuildAvailableAsync(SocketGuild arg)
    {
        return _interactionService.AddModulesToGuildAsync(arg, true, _interactionService.Modules.ToArray());
    }

    private static async Task HandleComponentCommandResultAsync(ComponentCommandInfo componentInfo, IInteractionContext interactionContext, IResult result)
    {
        if (result.IsSuccess) return;
        var interaction = interactionContext.Interaction;

        switch (result.Error)
        {
            case InteractionCommandError.Exception:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.ConvertFailed:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.BadArgs:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.Unsuccessful:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.UnmetPrecondition:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed("Nem rendelkezel megfelelő jogokkal a parancs futtatásához")).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.ParseFailed:
            {
                await interaction.FollowupAsync(embed:
                    Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
        }
    }

    private static async Task HandleSlashCommandResultAsync(SlashCommandInfo commandInfo, IInteractionContext interactionContext,
        IResult result)
    {
        if (result.IsSuccess) return;

        var interaction = interactionContext.Interaction;

        switch (result.Error)
        {
            case InteractionCommandError.Exception:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.ConvertFailed:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.BadArgs:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.Unsuccessful:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.UnmetPrecondition:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed("Nem rendelkezel megfelelő jogokkal a parancs futtatásához")).ConfigureAwait(false);
                break;
            }
            case InteractionCommandError.ParseFailed:
            {
                await interaction.FollowupAsync(embed: Embeds.ErrorEmbed(result.ErrorReason)).ConfigureAwait(false);
                break;
            }
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(Client, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _provider).ConfigureAwait(false);
        }
        catch (Exception)
        {
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync().ConfigureAwait(false)).Unwrap().ConfigureAwait(false);
        }
    }
}