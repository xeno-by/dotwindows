namespace FarPod.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FarPod.Extensions;
    using SharePodLib;

    /// <summary>
    /// Track factory
    /// </summary>
    class IPodTrackFactory : IDisposable
    {
        private readonly List<string> _forDeleteList = new List<string>();

        private readonly MaskMatcher _mmForMedia;
        private readonly MaskMatcher _mmForArt;

        public IPodTrackFactory()
        {
            _mmForMedia = new MaskMatcher(FarPodSetting.Default.SupportFileMask);
            _mmForArt = new MaskMatcher(FarPodSetting.Default.SupportArtWorkMask);
        }

        public NewTrack Get(string filePath)
        {
            var file = new FileInfo(filePath);

            if (!_mmForMedia.Compare(file.Name))
            {
                return null;
            }

            var newTrack = new NewTrack();

            using (TagLib.File mediaFile = TagLib.File.Create(file.FullName))
            {                
                newTrack.FilePath = file.FullName;

                newTrack.Artist = mediaFile.Tag.FirstPerformer;
                newTrack.Year = mediaFile.Tag.Year;
                newTrack.Album = mediaFile.Tag.Album;
                newTrack.TrackNumber = mediaFile.Tag.Track;
                newTrack.Title = mediaFile.Tag.Title;
                newTrack.Genre = mediaFile.Tag.FirstGenre;
                newTrack.AlbumTrackCount = mediaFile.Tag.TrackCount;
                newTrack.DiscNumber = mediaFile.Tag.Disc;
                newTrack.TotalDiscCount = mediaFile.Tag.DiscCount;
                newTrack.AlbumArtist = mediaFile.Tag.FirstAlbumArtist;
                newTrack.Composer = mediaFile.Tag.FirstComposer;
                newTrack.Comments = mediaFile.Tag.Comment;

                newTrack.Length = (uint)mediaFile.Properties.Duration.TotalMilliseconds;
                newTrack.Bitrate = (uint)mediaFile.Properties.AudioBitrate;
                newTrack.IsVideo = mediaFile.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video);

                if (string.IsNullOrEmpty(newTrack.Title)) newTrack.Title = Path.GetFileNameWithoutExtension(file.Name);

                bool artworkFileInTags = false;

                string artworkFile;

                if (mediaFile.Tag.Pictures.Length > 0 &&
                    mediaFile.Tag.Pictures[0].Data.Count > 100)
                {
                    artworkFile = Path.GetTempFileName();

                    File.WriteAllBytes(artworkFile, mediaFile.Tag.Pictures[0].Data.Data);

                    artworkFileInTags = true;

                    newTrack.ArtworkFile = artworkFile;

                    _forDeleteList.Add(artworkFile);
                }

                if (!artworkFileInTags)
                {
                    string artFile = Directory
                        .GetFiles(file.Directory.FullName)
                        .Where(f => _mmForArt.Compare(f))
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(artFile))
                    {
                        artworkFile = Path.Combine(file.Directory.FullName, artFile);

                        if (File.Exists(artworkFile)) newTrack.ArtworkFile = artworkFile;
                    }
                }

            }

            return newTrack;
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (string fileName in _forDeleteList)
            {
                if (File.Exists(fileName))
                {
                    File.SetAttributes(fileName, FileAttributes.Normal);
                    File.Delete(fileName);
                }
            }

            _forDeleteList.Clear();
        }

        #endregion
    }
}
