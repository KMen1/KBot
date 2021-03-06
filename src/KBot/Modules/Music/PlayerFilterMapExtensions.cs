using Lavalink4NET.Filters;
using Lavalink4NET.Player;

namespace KBot.Modules.Music;

public static class PlayerFilterMapExtensions
{
    public static void Clear(this PlayerFilterMap map)
    {
        map.Distortion = null;
        map.Equalizer = null;
        map.Karaoke = null;
        map.Rotation = null;
        map.Timescale = null;
        map.Tremolo = null;
        map.Vibrato = null;
        map.ChannelMix = null;
        map.LowPass = null;
    }

    public static void EnableBassBoost(this PlayerFilterMap map)
    {
        map.Equalizer = new EqualizerFilterOptions
        {
            Bands = new EqualizerBand[] { new(0, 0.2f), new(1, 0.2f), new(2, 0.2f) }
        };
    }

    public static void EnablePop(this PlayerFilterMap map)
    {
        map.Equalizer = new EqualizerFilterOptions
        {
            Bands = new EqualizerBand[]
            {
                new(0, 0.65f),
                new(1, 0.45f),
                new(2, -0.25f),
                new(3, -0.25f),
                new(4, -0.25f),
                new(5, 0.45f),
                new(6, 0.55f),
                new(7, 0.6f),
                new(8, 0.6f),
                new(9, 0.6f)
            }
        };
    }

    public static void EnableSoft(this PlayerFilterMap map)
    {
        map.Equalizer = new EqualizerFilterOptions
        {
            Bands = new EqualizerBand[]
            {
                new(8, -0.25f),
                new(9, -0.25f),
                new(10, -0.25f),
                new(11, -0.25f),
                new(12, -0.25f),
                new(13, -0.25f)
            }
        };
    }

    public static void EnableTreblebass(this PlayerFilterMap map)
    {
        map.Equalizer = new EqualizerFilterOptions
        {
            Bands = new EqualizerBand[]
            {
                new(0, 0.6f),
                new(1, 0.67f),
                new(2, 0.67f),
                new(4, -0.2f),
                new(5, 0.15f),
                new(6, -0.25f),
                new(7, 0.23f),
                new(8, 0.35f),
                new(9, 0.45f),
                new(10, 0.55f),
                new(11, 0.6f),
                new(12, 0.55f)
            }
        };
    }

    public static void EnableNightcore(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 1.165f,
            Pitch = 1.125f,
            Rate = 1.05f
        };
    }

    public static void EnableEightd(this PlayerFilterMap map)
    {
        map.Rotation = new RotationFilterOptions { Frequency = 0.2f };
    }

    public static void EnableVaporwave(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 1.0f,
            Pitch = 0.5f,
            Rate = 1.0f
        };
    }

    public static void EnableDoubletime(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 2.0f,
            Pitch = 1.0f,
            Rate = 1.0f
        };
    }

    public static void EnableSlowmotion(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 0.5f,
            Pitch = 1.0f,
            Rate = 0.8f
        };
    }

    public static void EnableChipmunk(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 1.05f,
            Pitch = 1.35f,
            Rate = 1.25f
        };
    }

    public static void EnableDarthvader(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 0.975f,
            Pitch = 0.5f,
            Rate = 0.8f
        };
    }

    public static void EnableDance(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 1.25f,
            Pitch = 1.25f,
            Rate = 1.25f
        };
    }

    public static void EnableChina(this PlayerFilterMap map)
    {
        map.Timescale = new TimescaleFilterOptions
        {
            Speed = 0.75f,
            Pitch = 1.25f,
            Rate = 1.25f
        };
    }

    public static void EnableVibrato(this PlayerFilterMap map)
    {
        map.Vibrato = new VibratoFilterOptions { Frequency = 4.0f, Depth = 0.75f };
    }

    public static void EnableTremolo(this PlayerFilterMap map)
    {
        map.Tremolo = new TremoloFilterOptions { Frequency = 4.0f, Depth = 0.75f };
    }
}
