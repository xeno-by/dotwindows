namespace FarPod.Helpers
{
    using System.IO;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Track name formatter
    /// </summary>
    static class IPodTrackFormatter
    {
        public static string Get(Track track, string format, bool escapeName = false)
        {            
            string newName = format;

            newName = newName.Replace("[Title]", getSafeTag(track.Title, escapeName));
            newName = newName.Replace("[Artist]", getSafeTag(track.Artist, escapeName));
            newName = newName.Replace("[Album]", getSafeTag(track.Album, escapeName));
            newName = newName.Replace("[TrackNumber]", track.TrackNumber.ToString("00"));
            newName = newName.Replace("[Genre]", getSafeTag(track.Genre, escapeName));
            newName = newName.Replace("[Composer]", getSafeTag(track.Composer, escapeName));
            newName = newName.Replace("[AlbumArtist]", getSafeTag(track.AlbumArtist, escapeName));
            newName = newName.Replace("[DiscNumber]", track.DiscNumber.ToString());
            newName = newName.Replace("[Year]", track.Year.ToString());

            while (newName.Contains(" \\"))
            {
                newName = newName.Replace(" \\", "\\");
            }

            return newName;
        }

        public static string Get(NewTrack track, string format, bool escapeName = false)
        {            
            string newName = format;

            newName = newName.Replace("[Title]", getSafeTag(track.Title, escapeName));
            newName = newName.Replace("[Artist]", getSafeTag(track.Artist, escapeName));
            newName = newName.Replace("[Album]", getSafeTag(track.Album, escapeName));
            newName = newName.Replace("[TrackNumber]", track.TrackNumber.ToString("00"));
            newName = newName.Replace("[Genre]", getSafeTag(track.Genre, escapeName));
            newName = newName.Replace("[Composer]", getSafeTag(track.Composer, escapeName));
            newName = newName.Replace("[AlbumArtist]", getSafeTag(track.AlbumArtist, escapeName));
            newName = newName.Replace("[DiscNumber]", track.DiscNumber.ToString());
            newName = newName.Replace("[Year]", track.Year.ToString());

            while (newName.Contains(" \\"))
            {
                newName = newName.Replace(" \\", "\\");
            }

            return newName;
        }

        public static bool HasMetaInfo(string format)
        {
            return
                format.Contains("[Title]") ||
                format.Contains("[Artist]") ||
                format.Contains("[Album]") ||
                format.Contains("[TrackNumber]") ||
                format.Contains("[Genre]") ||
                format.Contains("[Composer]") ||
                format.Contains("[AlbumArtist]") ||
                format.Contains("[DiscNumber]") ||
                format.Contains("[Year]");
        }

        private static string getSafeTag(string tag, bool escapeName)
        {
            if (tag == null) tag = string.Empty;

            if (escapeName)
            {                
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    tag = tag.Replace(c, '_');
                }
            }

            tag = tag.Trim();

            return string.IsNullOrEmpty(tag) ? MsgStr.Unknown : tag;
        }
    }
}
