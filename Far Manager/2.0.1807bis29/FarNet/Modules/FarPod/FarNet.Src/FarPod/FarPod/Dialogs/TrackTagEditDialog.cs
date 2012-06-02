namespace FarPod.Dialogs
{
    using System.Collections.Generic;
    using System.IO;
    using FarNet.Forms;
    using FarNet.Tools.ViewBuilder.Binding.Enums;
    using FarPod.Dialogs.Bases;
    using FarPod.Helpers;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Track tag edit dialog impl
    /// </summary>
    class TrackTagEditDialog : BaseView
    {
        private TagLib.File _currentMediaFile;
        private Track _currentTrack;

        private IPod _device;

        private int _currentIndex;
        private readonly int _itemCount;

        private readonly List<Track> _trackList;
        private readonly List<string> _fileList;

        private readonly List<TagLib.File> _filesToSave = new List<TagLib.File>();

        private bool _isDirty;

        #region TagsProperties

        public string Artist
        {
            get
            {
                return _currentMediaFile.Tag.FirstPerformer;
            }
            set
            {
                _currentMediaFile.Tag.Performers = getArray(value);

                if (_currentTrack != null) _currentTrack.Artist = value;

                _isDirty = true;
            }
        }

        public string Year
        {
            // int
            get
            {
                return _currentMediaFile.Tag.Year.ToString("0000");
            }
            set
            {
                _currentMediaFile.Tag.Year = getInt(value);

                if (_currentTrack != null) _currentTrack.Year = getInt(value);

                _isDirty = true;
            }
        }

        public string Album
        {
            get
            {
                return _currentMediaFile.Tag.Album;
            }
            set
            {
                _currentMediaFile.Tag.Album = value;

                if (_currentTrack != null) _currentTrack.Album = value;

                _isDirty = true;
            }
        }

        public string TrackNumber
        {
            // int
            get
            {
                return _currentMediaFile.Tag.Track.ToString("0000");
            }
            set
            {
                _currentMediaFile.Tag.Track = getInt(value);

                if (_currentTrack != null) _currentTrack.TrackNumber = getInt(value);

                _isDirty = true;
            }
        }

        public string Title
        {
            get
            {
                return _currentMediaFile.Tag.Title;
            }
            set
            {
                _currentMediaFile.Tag.Title = value;

                if (_currentTrack != null) _currentTrack.Title = value;

                _isDirty = true;
            }
        }

        public string Genre
        {
            get
            {
                return _currentMediaFile.Tag.FirstGenre;
            }
            set
            {
                _currentMediaFile.Tag.Genres = getArray(value);

                if (_currentTrack != null) _currentTrack.Genre = value;

                _isDirty = true;
            }
        }

        public string AlbumTrackCount
        {
            // int
            get
            {
                return _currentMediaFile.Tag.TrackCount.ToString("0000");
            }
            set
            {
                _currentMediaFile.Tag.TrackCount = getInt(value);

                if (_currentTrack != null) _currentTrack.AlbumTrackCount = getInt(value);

                _isDirty = true;
            }
        }

        public string DiscNumber
        {
            // int
            get
            {
                return _currentMediaFile.Tag.Disc.ToString("0000");
            }
            set
            {
                _currentMediaFile.Tag.Disc = getInt(value);

                if (_currentTrack != null) _currentTrack.DiscNumber = getInt(value);

                _isDirty = true;
            }
        }

        public string TotalDiscCount
        {
            // int
            get
            {
                return _currentMediaFile.Tag.DiscCount.ToString("0000");
            }
            set
            {
                _currentMediaFile.Tag.DiscCount = getInt(value);

                if (_currentTrack != null) _currentTrack.TotalDiscCount = getInt(value);

                _isDirty = true;
            }
        }

        public string AlbumArtist
        {
            get
            {
                return _currentMediaFile.Tag.FirstAlbumArtist;
            }
            set
            {
                _currentMediaFile.Tag.AlbumArtists = getArray(value);

                if (_currentTrack != null) _currentTrack.AlbumArtist = value;

                _isDirty = true;
            }
        }

        public string Composer
        {
            get
            {
                return _currentMediaFile.Tag.FirstComposer;
            }
            set
            {
                _currentMediaFile.Tag.Composers = getArray(value);

                if (_currentTrack != null) _currentTrack.Composer = value;

                _isDirty = true;
            }
        }

        public string Comments
        {
            get
            {
                return _currentMediaFile.Tag.Comment;
            }
            set
            {
                _currentMediaFile.Tag.Comment = value;

                if (_currentTrack != null) _currentTrack.Comment = value;

                _isDirty = true;
            }
        }

        #endregion

        public bool IsSingleFileMode
        {
            get
            {
                return _itemCount <= 1;
            }
        }

        private TrackTagEditDialog()
        {

        }

        public TrackTagEditDialog(List<string> fileNameList)
            : this()
        {
            _fileList = fileNameList;
            _itemCount = fileNameList.Count;
        }

        public TrackTagEditDialog(IPod dev, List<Track> trackList)
            : this()
        {
            _trackList = trackList;
            _itemCount = trackList.Count;

            _device = dev;

            _device.AssertIsWritable();
            _device.AcquireLock();
        }

        protected override void initView()
        {
            loadCurrent();

            base.initView();
        }

        public override bool Show()
        {
            bool dlgResult = base.Show();

            if (dlgResult)
            {
                saveCurrent();

                if (_filesToSave.Count > 0)
                {
                    ProgressFormHelper.Invoke(f =>
                    {
                        foreach (var mediaFile in _filesToSave)
                        {
                            mediaFile.Save();
                        }

                        _filesToSave.Clear();
                    },
                    MsgStr.MsgSaveFileTags);

                    if (_device != null)
                    {
                        ProgressFormHelper.Invoke(
                            f => _device.SaveChanges(),
                            MsgStr.MsgSaveIPodDB);
                    }
                }
            }

            return dlgResult;
        }

        protected void onPrev(object sender, ButtonClickedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                saveCurrent();

                _currentIndex--;

                loadCurrent();
            }
        }

        protected void onNext(object sender, ButtonClickedEventArgs e)
        {
            if (_currentIndex < _itemCount - 1)
            {
                saveCurrent();

                _currentIndex++;

                loadCurrent();
            }
        }

        private void saveCurrent()
        {
            viewFactory.UpdateSource(EBingingMode.TwoWayManual);

            if (_isDirty)
            {
                if (!_filesToSave.Contains(_currentMediaFile)) _filesToSave.Add(_currentMediaFile);
            }
        }

        private void loadCurrent()
        {
            if (_trackList != null)
            {
                _currentTrack = _trackList[_currentIndex];
                _currentMediaFile = TagLib.File.Create(Path.Combine(_device.FileSystem.DriveLetter, _currentTrack.FilePath));
            }
            else if (_fileList != null)
            {
                _currentTrack = null;
                _currentMediaFile = TagLib.File.Create(_fileList[_currentIndex]);
            }

            _isDirty = false;

            firePropertyChanged("*");
        }

        private string[] getArray(string value)
        {
            return new [] { value };
        }

        private uint getInt(string value)
        {
            return uint.Parse(value);
        }

        public override void Dispose()
        {
            if (_currentMediaFile != null)
            {
                _currentMediaFile.Dispose();
                _currentMediaFile = null;
            }

            if (_device != null)
            {
                _device.ReleaseLock();
                _device = null;
            }

            _filesToSave.Clear();

            base.Dispose();
        }
    }
}
