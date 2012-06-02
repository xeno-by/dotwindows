namespace FarPod.Extensions
{
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Generic IPod Extensions
    /// </summary>
    static class IPodExtensions
    {
        public static Track GetTrackIfExists(this IPod dev, NewTrack newTrack)
        {
            foreach (Track existing in dev.Tracks)
            {
                if (existing.Title == newTrack.Title &&
                    existing.Artist == newTrack.Artist &&
                    existing.Album == newTrack.Album &&
                    existing.TrackNumber == newTrack.TrackNumber)
                {
                    return existing;
                }
            }

            return null;
        }

        public static bool IsEqualToDevice(this IPod f, IPod s)
        {
            return f.DeviceInfo.SerialNumber == s.DeviceInfo.SerialNumber &&
                   f.FileSystem.DriveLetter == s.FileSystem.DriveLetter;
        }
    }
}
