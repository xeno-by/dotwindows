namespace FarPod.Dialogs
{
    using System.Collections.Generic;
    using System.IO;
    using FarNet.Tools.ViewBuilder.Binding.Enums;
    using FarPod.Dialogs.Bases;
    using FarPod.Helpers;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Batch tag edit dialog impl
    /// </summary>
    class BatchTagEditDialog : BaseView
    {
        private readonly List<Track> _trackList;
        private readonly List<string> _fileList;

        private readonly IPod _device;

        #region TagsProperties

        public bool SetArtist { get; set; }

        public string Artist
        {
            get;
            set;
        }

        public bool SetYear { get; set; }

        public string Year
        {
            // int
            get;
            set;
        }

        public bool SetAlbum { get; set; }

        public string Album
        {
            get;
            set;
        }

        public bool SetTrackNumber { get; set; }

        public string TrackNumber
        {
            // int
            get;
            set;
        }

        public bool SetTitle { get; set; }

        public string Title
        {
            get;
            set;
        }

        public bool SetGenre { get; set; }

        public string Genre
        {
            get;
            set;
        }

        public bool SetAlbumTrackCount { get; set; }

        public string AlbumTrackCount
        {
            // int
            get;
            set;
        }

        public bool SetDiscNumber { get; set; }

        public string DiscNumber
        {
            // int
            get;
            set;
        }

        public bool SetTotalDiscCount { get; set; }

        public string TotalDiscCount
        {
            // int
            get;
            set;
        }

        public bool SetAlbumArtist { get; set; }

        public string AlbumArtist
        {
            get;
            set;
        }

        public bool SetComposer { get; set; }

        public string Composer
        {
            get;
            set;
        }

        public bool SetComments { get; set; }

        public string Comments
        {
            get;
            set;
        }

        #endregion

        public BatchTagEditDialog(List<string> fileNameList)
        {
            _fileList = fileNameList;
        }

        public BatchTagEditDialog(IPod dev, List<Track> trackList)
        {
            _trackList = trackList;

            _device = dev;
        }

        public override bool Show()
        {
            bool dlgResult = base.Show();

            if (dlgResult)
            {
                viewFactory.UpdateSource(EBingingMode.TwoWayManual);

                if (_trackList != null)
                {
                    try
                    {
                        _device.AssertIsWritable();
                        _device.AcquireLock();

                        ProgressFormHelper.Invoke(f =>
                        {
                            foreach (var tr in _trackList)
                            {
                                setMediaFileInfo(
                                    TagLib.File.Create(Path.Combine(_device.FileSystem.DriveLetter, tr.FilePath)),
                                    tr);
                            }
                        },
                        MsgStr.MsgSaveFileTags);

                        ProgressFormHelper.Invoke(
                            f => _device.SaveChanges(),
                            MsgStr.MsgSaveIPodDB);
                    }
                    finally
                    {
                        _device.ReleaseLock();
                    }
                }
                else if (_fileList != null)
                {
                    ProgressFormHelper.Invoke(f =>
                        {
                            foreach (var ff in _fileList)
                            {
                                setMediaFileInfo(
                                    TagLib.File.Create(ff),
                                    null);
                            }
                        },
                        MsgStr.MsgSaveFileTags);
                }
            }

            return dlgResult;
        }

        private string[] getArray(string value)
        {
            return new[] { value };
        }

        private uint getInt(string value)
        {
            return uint.Parse(value);
        }

        private void setMediaFileInfo(TagLib.File mf, Track tr)
        {
            if (SetArtist)
            {
                if (mf != null) mf.Tag.Performers = getArray(Artist);
                if (tr != null) tr.Artist = Artist;
            }
            if (SetYear)
            {
                if (mf != null) mf.Tag.Year = getInt(Year);
                if (tr != null) tr.Year = getInt(Year);
            }
            if (SetAlbum)
            {
                if (mf != null) mf.Tag.Album = Album;
                if (tr != null) tr.Album = Album;
            }
            if (SetTrackNumber)
            {
                if (mf != null) mf.Tag.Track = getInt(TrackNumber);
                if (tr != null) tr.TrackNumber = getInt(TrackNumber);
            }
            if (SetTitle)
            {
                if (mf != null) mf.Tag.Title = Title;
                if (tr != null) tr.Title = Title;
            }
            if (SetGenre)
            {
                if (mf != null) mf.Tag.Genres = getArray(Genre);
                if (tr != null) tr.Genre = Genre;
            }
            if (SetAlbumTrackCount)
            {
                if (mf != null) mf.Tag.TrackCount = getInt(AlbumTrackCount);
                if (tr != null) tr.AlbumTrackCount = getInt(AlbumTrackCount);
            }
            if (SetDiscNumber)
            {
                if (mf != null) mf.Tag.Disc = getInt(DiscNumber);
                if (tr != null) tr.DiscNumber = getInt(DiscNumber);
            }
            if (SetTotalDiscCount)
            {
                if (mf != null) mf.Tag.DiscCount = getInt(TotalDiscCount);
                if (tr != null) tr.TotalDiscCount = getInt(TotalDiscCount);
            }
            if (SetAlbumArtist)
            {
                if (mf != null) mf.Tag.AlbumArtists = getArray(AlbumArtist);
                if (tr != null) tr.AlbumArtist = AlbumArtist;
            }
            if (SetComposer)
            {
                if (mf != null) mf.Tag.Composers = getArray(Composer);
                if (tr != null) tr.Composer = Composer;
            }
            if (SetComments)
            {
                if (mf != null) mf.Tag.Comment = Comments;
                if (tr != null) tr.Comment = Comments;
            }
        }
    }
}
