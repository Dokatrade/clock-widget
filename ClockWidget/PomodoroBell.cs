using System.IO;
using System.Windows.Media;

namespace ClockWidget;

public enum PomodoroSound
{
    FreesoundsNotification,
    Harp,
    Ladder,
    MusicBox,
    MessageNotification,
    NewNotification015,
    NewNotification036,
    NewNotification059
}

internal static class PomodoroBell
{
    private static readonly MediaPlayer Player = new();

    private static readonly IReadOnlyDictionary<PomodoroSound, string> FileNames =
        new Dictionary<PomodoroSound, string>
        {
            [PomodoroSound.FreesoundsNotification] = "freesounds123-notification-sounds-351833.mp3",
            [PomodoroSound.Harp] = "harp.mp3",
            [PomodoroSound.Ladder] = "ladder.mp3",
            [PomodoroSound.MusicBox] = "music_box.mp3",
            [PomodoroSound.MessageNotification] = "notification_message-notification-5-337824.mp3",
            [PomodoroSound.NewNotification015] = "universfield-new-notification-015-363677.mp3",
            [PomodoroSound.NewNotification036] = "universfield-new-notification-036-485897.mp3",
            [PomodoroSound.NewNotification059] = "universfield-new-notification-059-494262.mp3"
        };

    public static void Play(PomodoroSound sound)
    {
        var path = GetSoundPath(sound);
        if (!File.Exists(path))
        {
            return;
        }

        Player.Stop();
        Player.Open(new Uri(path, UriKind.Absolute));
        Player.Position = TimeSpan.Zero;
        Player.Play();
    }

    private static string GetSoundPath(PomodoroSound sound)
    {
        var fileName = FileNames.TryGetValue(sound, out var value)
            ? value
            : FileNames[PomodoroSound.FreesoundsNotification];

        return Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", fileName);
    }
}
