﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using KBot.Enums;
using KBot.Helpers;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Filters;
using Victoria.Responses.Search;

// ReSharper disable InconsistentNaming

namespace KBot.Services;

public class AudioService
{
    private readonly DiscordSocketClient _client;
    private readonly LavaNode _lavaNode;

    private readonly List<LavaTrack> previousTracks = new();
    private readonly List<string> enabledFilters = new();
    private LavaTrack currentTrack;
    private IUserMessage nowPlayingMessage;
    private IDiscordInteraction currentInteraction;
    private bool isloopEnabled;

    public AudioService(DiscordSocketClient client, LavaNode lavaNode)
    {
        _lavaNode = lavaNode;
        _client = client;
    }

    public void InitializeAsync()
    {
        _client.Ready += OnReadyAsync;
        _client.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
        _lavaNode.OnTrackEnded += OnTrackEnded;
        _lavaNode.OnTrackException += OnTrackException;
    }

    private async Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        if (user.Id == _client.CurrentUser.Id && after.VoiceChannel == null)
        {
            await StopAsync(before.VoiceChannel.Guild);
            await LeaveAsync(before.VoiceChannel.Guild);
            var msg = await GetNowPlayingMessage();
            if (msg != null)
            {
                await msg.Channel.SendMessageAsync(
                    embed: await EmbedHelper.MakeError("Egy barom lecsatlakoztatott a hangcsatornából."));
                await msg.DeleteAsync();
                ResetPlayer();
            }
        }
    }

    

    private async Task OnReadyAsync()
    {
        await _lavaNode.ConnectAsync();
    }

    private async Task OnTrackException(TrackExceptionEventArgs arg)
    {
        ResetPlayer();
        await arg.Player.StopAsync();
        await arg.Player.ApplyFiltersAsync(FilterHelper.DefaultFilters());
        await arg.Player.TextChannel.SendMessageAsync(embed: await EmbedHelper.MakeError(arg.Exception.Message));
        await _lavaNode.LeaveAsync(arg.Player.VoiceChannel);
    }

    public async Task<Embed> JoinAsync(IGuild guild, ITextChannel tChannel, SocketUser user)
    {
        if (_lavaNode.HasPlayer(guild))
        {
            return await EmbedHelper.MakeError("Már csatlakozva vagyok valahova!");
        }
        var voiceChannel = ((IVoiceState) user).VoiceChannel;
        if (voiceChannel is null)
        {
            return await EmbedHelper.MakeError("Nem vagy hangcsatornában!");
        }
        await _lavaNode.JoinAsync(voiceChannel, tChannel);
        return await EmbedHelper.MakeJoin(voiceChannel);
    }

    public async Task<Embed> LeaveAsync(IGuild guild)
    {
        if (!_lavaNode.HasPlayer(guild))
        {
            return await EmbedHelper.MakeError("Ezen a szerveren nem található lejátszó!");
        }
        var player = _lavaNode.GetPlayer(guild);
        var voiceChannel = player.VoiceChannel;
        await _lavaNode.LeaveAsync(voiceChannel);
        var msg = await GetNowPlayingMessage();
        await msg.DeleteAsync();
        return await EmbedHelper.MakeLeave(voiceChannel);
    }

    public async Task<Embed> MoveAsync(IGuild guild, SocketUser user)
    {
        if (!_lavaNode.HasPlayer(guild))
        {
            return await EmbedHelper.MakeError("Ezen a szerveren nem található lejátszó!");
        }
        var voiceChannel = ((IVoiceState) user).VoiceChannel;
        if (voiceChannel is null)
        {
            return await EmbedHelper.MakeError("Nem vagy hangcsatornában!");
        }
        await _lavaNode.MoveChannelAsync(voiceChannel);
        return await EmbedHelper.MakeMove(voiceChannel);
    }

    public async Task<(Embed, MessageComponent, bool)> PlayAsync(string query, IGuild guild,
        ITextChannel tChannel, SocketUser user, IDiscordInteraction interaction)
    {
        var search = Uri.IsWellFormedUriString(query, UriKind.Absolute)
            ? await _lavaNode.SearchAsync(SearchType.Direct, query)
            : await _lavaNode.SearchYouTubeAsync(query);
        if (search.Status == SearchStatus.NoMatches)
        {
            return (await EmbedHelper.MakeError("Nincs találat!"), null, false);
        }
        var track = search.Tracks.FirstOrDefault();
        var voiceChannel = ((IVoiceState)user).VoiceChannel;
        if (voiceChannel is null) return (await EmbedHelper.MakeError("Nem vagy hangcsatornában!"), null, false);
        var player = _lavaNode.HasPlayer(guild) ? _lavaNode.GetPlayer(guild) : await _lavaNode.JoinAsync(voiceChannel, tChannel);
        
        
        if (IsPlaying(player))
        {
            player.Queue.Enqueue(track);
            var newComponents = await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
            await ModifyNowPlayingMessage(null, newComponents);
            
            return (await EmbedHelper.MakeAddedToQueue(track, player), null, true);
        }

        currentInteraction ??= interaction;
        await player.PlayAsync(track);
        await player.UpdateVolumeAsync(100);
        currentTrack = track;
        return (
            await EmbedHelper.MakeNowPlaying(user, player, isloopEnabled, player.Volume, enabledFilters),
            await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState), false);
    }

    public async Task StopAsync(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) return;
        
        await player.StopAsync();
        ResetPlayer();
    }

    public async Task PlayNextTrack(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null || player.Queue.Count == 0)
        {
            return;
        }
        previousTracks.Add(currentTrack);
        await player.SkipAsync();
        
        var newEmbed = await EmbedHelper.MakeNowPlaying(user, player, isloopEnabled, player.Volume, enabledFilters);
        var newComponents = await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
        await ModifyNowPlayingMessage(newEmbed, newComponents);
    }

    public async Task PlayPreviousTrack(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        var prev = previousTracks.Last();
        await player.PlayAsync(prev);
        previousTracks.Remove(prev);
        
        var newEmbed = await EmbedHelper.MakeNowPlaying(user, player, isloopEnabled, player.Volume, enabledFilters);
        var newComponents = await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
        await ModifyNowPlayingMessage(newEmbed, newComponents);
    }

    public async Task PauseOrResumeAsync(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) return;

        switch (player.PlayerState)
        {
            case PlayerState.Playing:
            {
                await player.PauseAsync();
                var newComponents = await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
                await ModifyNowPlayingMessage(null, newComponents);
                break;
            }
            case PlayerState.Paused:
            {
                await player.ResumeAsync();
                var newComponents = await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
                await ModifyNowPlayingMessage(null, newComponents);
                break;
            }
        }
    }

    public async Task SetVolumeAsync(IGuild guild, VoiceButtonType buttonType)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) return;
        
        var currentVolume = player.Volume;
        
        switch (currentVolume)
        {
            case 0 when buttonType == VoiceButtonType.VolumeDown:
            case 100 when buttonType == VoiceButtonType.VolumeUp:
                return;
            default:
                switch (buttonType)
                {
                    case VoiceButtonType.VolumeUp:
                    {
                        var newVolume = currentVolume + 10;
                        await player.UpdateVolumeAsync((ushort) newVolume);
                        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, newVolume, enabledFilters);
                        await ModifyNowPlayingMessage(newEmbed, null);
                        break;
                    }
                    case VoiceButtonType.VolumeDown:
                    {
                        var newVolume = currentVolume - 10;
                        await player.UpdateVolumeAsync((ushort) newVolume);
                        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, newVolume, enabledFilters);
                        await ModifyNowPlayingMessage(newEmbed, null);
                        break;
                    }
                }
                break;
        }
    }
    
    public async Task<Embed> SetVolumeAsync(ushort volume, IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) return await EmbedHelper.MakeError("A lejátszó nem található!");

        await player.UpdateVolumeAsync(volume);
        var newEmbed1 = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, volume, enabledFilters);
        await ModifyNowPlayingMessage(newEmbed1, null);
        return await EmbedHelper.MakeVolume(player, volume);
    }

    public async Task<Embed> SetFiltersAsync(IGuild guild, IEnumerable<IFilter> filters, EqualizerBand[] bands, string[] filtersName)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) 
            return await EmbedHelper.MakeError("A lejátszó nem található!");
        await player.ApplyFiltersAsync(filters, equalizerBands:bands);
        enabledFilters.Clear();
        enabledFilters.AddRange(filtersName);
        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, player.Volume, enabledFilters);
        await ModifyNowPlayingMessage(newEmbed, null);
        return await EmbedHelper.MakeFilter(filtersName);
    }
    
    public async Task SetRepeatAsync(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        isloopEnabled = !isloopEnabled;
        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, player.Volume, enabledFilters);
        var newComponents =
            await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
        await ModifyNowPlayingMessage(newEmbed, newComponents);
    }

    public async Task<Embed> SetSpeedAsync(float value, IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing})
            return await EmbedHelper.MakeError("Jelenleg nincs zene lejátszás alatt!");
        await player.ApplyFilterAsync(FilterHelper.Speed(value));
        enabledFilters.Add($"Speed: {value}");
        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, player.Volume, enabledFilters);
        await ModifyNowPlayingMessage(newEmbed, null);
        return await EmbedHelper.MakeFilter(new [] {$"SEBESSÉG -> {value}"});
    }

    public async Task<Embed> SetPitchAsync(float value, IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing})
            return await EmbedHelper.MakeError("Jelenleg nincs zene lejátszás alatt!");
        await player.ApplyFilterAsync(FilterHelper.Pitch(value));
        enabledFilters.Add($"Pitch: {value}");
        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, player.Volume, enabledFilters);
        await ModifyNowPlayingMessage(newEmbed, null);
        return await EmbedHelper.MakeFilter(new [] {$"HANGMAGASSÁG -> {value}"});
    }

    public async Task ClearFiltersAsync(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing}) return;
        
        await player.ApplyFiltersAsync(FilterHelper.DefaultFilters());
        enabledFilters.Clear();
        var newEmbed = await EmbedHelper.MakeNowPlaying((SocketUser)currentInteraction.User, player, isloopEnabled, player.Volume, enabledFilters);
        await ModifyNowPlayingMessage(newEmbed, null);
    }
    
    public async Task<Embed> GetQueue(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) return await EmbedHelper.MakeQueue(null, true);

        return await EmbedHelper.MakeQueue(player);
    }

    public async Task<Embed> ClearQueue(IGuild guild)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is null) 
            return await EmbedHelper.MakeError("A lejátszó nem található!");
        player.Queue.Clear();
        return await EmbedHelper.MakeQueue(player, true);
    }

    private async Task OnTrackEnded(TrackEndedEventArgs args)
    {
        if (!ShouldPlayNext(args.Reason)) return;
        previousTracks.Add(args.Track);
        var player = args.Player;
        if (!player.Queue.TryDequeue(out var queueable))
        {
            if (isloopEnabled) await player.PlayAsync(args.Track);
            
            var msg = await GetNowPlayingMessage();
            await msg.DeleteAsync();
            await _lavaNode.LeaveAsync(player.VoiceChannel);
            ResetPlayer();
            return;
        }

        if (queueable is not { } track) return;
        await player.PlayAsync(track);
        var newEmbed = await EmbedHelper.MakeNowPlaying(_client.CurrentUser, player, isloopEnabled, player.Volume, enabledFilters);
        var newComponents =
            await ComponentHelper.MakeNowPlayingComponents(CanGoBack(), CanGoForward(player), player.PlayerState);
        await ModifyNowPlayingMessage(newEmbed, newComponents);
    }

    private async Task ModifyNowPlayingMessage(Embed embed, MessageComponent components)
    {
        var msg = await GetNowPlayingMessage();

        if (embed is not null && components is not null)
        {
            await msg.ModifyAsync(x =>
            {
                x.Embed = embed;
                x.Components = components;
            });
            nowPlayingMessage = msg;
        }
        else if (embed is not null)
        {
            await msg.ModifyAsync(x => x.Embed = embed);
            nowPlayingMessage = msg;
        }
        else if (components is not null)
        {
            await msg.ModifyAsync(x => x.Components = components);
            nowPlayingMessage = msg;
        }
    }
    
    private async Task<IUserMessage> GetNowPlayingMessage()
    {
        return nowPlayingMessage ?? await currentInteraction.GetOriginalResponseAsync();
    }
    
    private static bool ShouldPlayNext(TrackEndReason trackEndReason)
    {
        return trackEndReason is TrackEndReason.Finished or TrackEndReason.LoadFailed;
    }
    private bool CanGoBack()
    {
        return previousTracks.Count > 0;
    }
    
    private static bool CanGoForward(LavaPlayer player)
    {
        return player.Queue.Count > 0;
    }
    
    private static bool IsPlaying(LavaPlayer player)
    {
        return player.Track is not null && player.PlayerState is PlayerState.Playing ||
               player.PlayerState is PlayerState.Paused;
    }
    
    private void ResetPlayer()
    {
        enabledFilters.Clear();
        previousTracks.Clear();
        currentTrack = null;
        nowPlayingMessage = null;
        currentInteraction = null;
        isloopEnabled = false;
    }

}