﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

    private bool loop;
    
    public AudioService(DiscordSocketClient client, LavaNode lavaNode)
    {
        _lavaNode = lavaNode;
        _client = client;
    }

    public void InitializeAsync()
    {
        _client.Ready += OnReadyAsync;
        _lavaNode.OnTrackEnded += OnTrackEnded;
        _lavaNode.OnTrackException += OnTrackException;
    }
    private async Task OnReadyAsync()
    {
        await _lavaNode.ConnectAsync();
    }
    private async Task OnTrackException(TrackExceptionEventArgs arg)
    {
        await arg.Player.StopAsync();
        var eb = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = "HIBA TÖRTÉNT A ZENE LEJÁTSZÁSA KÖZBEN!",
                IconUrl = _client.CurrentUser.GetAvatarUrl()
            },
            Title = arg.Track.Title,
            Url = arg.Track.Url,
            Description = $"Kérlek próbáld meg újra lejátszani a zenét! \n" +
                          $"Ha a hiba továbbra is fennáll, kérlek jelezd a <@132797923049209856>-nek! \n",
                          //$"A bot beragadása esetén használd a **/reset** parancsot!",
            Color = Color.Red,
            Footer = new EmbedFooterBuilder
            {
                Text = "Dátum: " + DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss")
            }
        };
        eb.AddField("Hibaüzenet", $"```{arg.Exception.Message}```");
        await arg.Player.TextChannel.SendMessageAsync(embed:eb.Build());
        IFilter[] filters =
        {
            Filter.NightCore(false),
            Filter.EightD(false),
            Filter.VaporWave(false),
        };
        await arg.Player.ApplyFiltersAsync(filters, 1, Filter.BassBoost(false));
        await _lavaNode.LeaveAsync(arg.Player.VoiceChannel);
    }
    public async Task<Embed> JoinAsync(IGuild guild, IVoiceChannel vChannel, ITextChannel tChannel, SocketUser user)
    {
        if (_lavaNode.HasPlayer(guild) || vChannel is null) return await EmbedHelper.MakeJoin(user, vChannel, true);
        await _lavaNode.JoinAsync(vChannel, tChannel);
        return await EmbedHelper.MakeJoin(user, vChannel, false);
    }

    public async Task<Embed> LeaveAsync(IGuild guild, IVoiceChannel vChannel, SocketUser user)
    {
        if (!_lavaNode.HasPlayer(guild) || vChannel is null) return await EmbedHelper.MakeLeave(user, vChannel, true);
        await _lavaNode.LeaveAsync(vChannel);
        return await EmbedHelper.MakeLeave(user, vChannel, false);
    }

    public async Task<Embed> MoveAsync(IGuild guild, IVoiceChannel vChannel, SocketUser user)
    {
        if (!_lavaNode.HasPlayer(guild))
            return await EmbedHelper.MakeMove(user, _lavaNode.GetPlayer(guild), vChannel, true);
        await _lavaNode.MoveChannelAsync(vChannel);
        return await EmbedHelper.MakeMove(user, _lavaNode.GetPlayer(guild), vChannel, false);
    }

    public async Task<Embed> PlayAsync([Remainder] string query, IGuild guild, IVoiceChannel vChannel,
        ITextChannel tChannel, SocketUser user)
    {
        var search = Uri.IsWellFormedUriString(query, UriKind.Absolute)
            ? await _lavaNode.SearchAsync(SearchType.Direct, query)
            : await _lavaNode.SearchYouTubeAsync(query);
        var track = search.Tracks.FirstOrDefault();
        var player = _lavaNode.HasPlayer(guild)
            ? _lavaNode.GetPlayer(guild)
            : await _lavaNode.JoinAsync(vChannel, tChannel);
        if (search.Status == SearchStatus.NoMatches)
            return await EmbedHelper.MakePlay(user, track, player, null, true, false);

        if (player.Track != null && player.PlayerState is PlayerState.Playing ||
            player.PlayerState is PlayerState.Paused)
        {
            player.Queue.Enqueue(track);
            return await EmbedHelper.MakePlay(user, track, player, await track.FetchArtworkAsync(), false, true);
        }

        await player.PlayAsync(track);
        return await EmbedHelper.MakePlay(user, track, player, await track.FetchArtworkAsync(), false, false);
    }

    public async Task<Embed> StopAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeStop(user, null, true);
        await player.StopAsync();
        return await EmbedHelper.MakeStop(user, player, false);
    }

    public async Task<Embed> SkipAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null || player.Queue.Count == 0) return await EmbedHelper.MakeSkip(user, player, null, true);
        await player.SkipAsync();
        return await EmbedHelper.MakeSkip(user, player, await player.Track.FetchArtworkAsync(), false);
    }

    public async Task<Embed> PauseOrResumeAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);

        if (player == null) return await EmbedHelper.MakePauseOrResume(user, null, true, false);

        if (player.PlayerState == PlayerState.Playing)
        {
            await player.PauseAsync();
            return await EmbedHelper.MakePauseOrResume(user, player, false, true);
        }

        await player.ResumeAsync();
        return await EmbedHelper.MakePauseOrResume(user, player, false, false);
    }

    public async Task<Embed> SetVolumeAsync(ushort volume, IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeVolume(user, null, volume, true);
        await player.UpdateVolumeAsync(volume);
        return await EmbedHelper.MakeVolume(user, player, volume, false);
    }

    public async Task<Embed> SetBassBoostAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeFilter(user, null, "BASS BOOST", true);

        await player.EqualizerAsync(Filter.BassBoost(true));
        return await EmbedHelper.MakeFilter(user, player, "BASS BOOST", false);
    }

    public async Task<Embed> SetNightCoreAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeFilter(user, null, "NIGHTCORE", true);
        await player.ApplyFilterAsync(Filter.NightCore(true));
        return await EmbedHelper.MakeFilter(user, player, "NIGHTCORE", false);
    }

    public async Task<Embed> SetEightDAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeFilter(user, null, "8D", true);
        await player.ApplyFilterAsync(Filter.EightD(true));
        return await EmbedHelper.MakeFilter(user, player, "8D", false);
    }

    public async Task<Embed> SetVaporWaveAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeFilter(user, null, "VAPORWAVE", true);
        await player.ApplyFilterAsync(Filter.VaporWave(true));
        return await EmbedHelper.MakeFilter(user, player, "VAPORWAVE", false);
    }

    public async Task<Embed> SetLoopAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing}) return await EmbedHelper.MakeLoop(user, null, true);
        loop = !loop;
        return await EmbedHelper.MakeLoop(user, player, false);
    }

    public async Task<Embed> ClearFiltersAsync(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing})
            return await EmbedHelper.MakeFilter(user, null, "CLEAR", true);
        IFilter[] filters =
        {
            Filter.NightCore(false),
            Filter.EightD(false),
            Filter.VaporWave(false),
        };
        await player.ApplyFiltersAsync(filters, 1, Filter.BassBoost(false));
        return await EmbedHelper.MakeFilter(user, player, "MINDEN", false);
    }

    public async Task<Embed> SetSpeedAsync(float value, IGuild guild, SocketUser commandUser)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing})
            return await EmbedHelper.MakeFilter(commandUser, null, "SEBESSÉG", true);
        await player.ApplyFilterAsync(Filter.Speed(true, value));
        return await EmbedHelper.MakeFilter(commandUser, player, $"SEBESSÉG -> {value}", false);
    }

    public async Task<Embed> SetPitchAsync(float value, IGuild guild, SocketUser commandUser)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player is not {PlayerState: PlayerState.Playing})
            return await EmbedHelper.MakeFilter(commandUser, null, "HANGMAGASSÁG", true);
        await player.ApplyFilterAsync(Filter.Pitch(true, value));
        return await EmbedHelper.MakeFilter(commandUser, player, $"HANGMAGASSÁG -> {value}", false);
    }

    public async Task<Embed> FastForward(TimeSpan time, IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeFastForward(user, null, time, true);
        await player.SeekAsync(time);
        return await EmbedHelper.MakeFastForward(user, player, time, false);
    }

    private static bool ShouldPlayNext(TrackEndReason trackEndReason)
    {
        return trackEndReason is TrackEndReason.Finished or TrackEndReason.LoadFailed;
    }

    public async Task<Embed> GetQueue(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeQueue(user, null, true);

        return await EmbedHelper.MakeQueue(user, player, false);
    }

    public async Task<Embed> ClearQueue(IGuild guild, SocketUser user)
    {
        var player = _lavaNode.GetPlayer(guild);
        if (player == null) return await EmbedHelper.MakeQueue(user, null, true);
        player.Queue.Clear();
        return await EmbedHelper.MakeQueue(user, player, false, true);
    }

    private async Task OnTrackEnded(TrackEndedEventArgs args)
    {
        if (!ShouldPlayNext(args.Reason)) return;

        var player = args.Player;
        if (!player.Queue.TryDequeue(out var queueable))
        {
            if (loop)
                await player.PlayAsync(args.Track);
            //await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
            return;
        }

        if (queueable is not { } track)
            //await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
            return;
        await args.Player.PlayAsync(track);
        var eb = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = "KÖVETKEZŐ LEJÁTSZÁSA",
                IconUrl = _client.CurrentUser.GetAvatarUrl()
            },
            Color = Color.Green,
            Description = $"Ebben a csatornában: `{player.VoiceChannel.Name}`",
            Title = track.Title,
            Url = track.Url,
            ImageUrl = await track.FetchArtworkAsync(),
            Footer = new EmbedFooterBuilder
            {
                Text = $"Hossz -> {player.Track.Duration:hh\\:mm\\:ss}"
            }
        };
        await player.TextChannel.SendMessageAsync(string.Empty, false, eb.Build());
    }
}