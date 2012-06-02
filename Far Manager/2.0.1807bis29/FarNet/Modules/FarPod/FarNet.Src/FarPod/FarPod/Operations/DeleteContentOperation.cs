namespace FarPod.Operations
{
    using System.Collections.Generic;
    using System.Linq;
    using FarPod.Operations.Bases;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// DeleteContentOperation
    /// </summary>
    class DeleteContentOperation : OnDeviceOperation
    {
        private readonly IEnumerable<Playlist> _playlists;
        private readonly IEnumerable<Track> _tracks;

        private readonly Playlist _playList;

        private bool _isAltMode;

        // TODO: export to ENUM
        public int OptionalActionId { get; set; }

        private DeleteContentOperation(IPod dev, bool isAltMode)
            : base(dev)
        {
            _isAltMode = isAltMode;

            // ???
            isWriteMode = true;

            setText(MsgStr.MsgDelete);
        }

        public DeleteContentOperation(IPod dev, IEnumerable<Playlist> playlists, bool isAltMode)
            : this(dev, isAltMode)
        {
            _playList = null;

            _playlists = playlists;
        }

        public DeleteContentOperation(IPod dev, Playlist pl, IEnumerable<Track> tracks, bool isAltMode)
            : this(dev, isAltMode)
        {
            _playList = pl;

            _tracks = tracks;
        }

        protected override bool workOnDevice()
        {
            notProcessedItems.Clear();

            if (_playlists != null)
            {                
                int progress = 0;
                int totalCount = 0;

                foreach (Playlist pl in _playlists)
                {
                    notProcessedItems.Add(pl);
                }

                if (OptionalActionId == 1)
                    totalCount = _playlists.Sum(p => p.TrackCount);
                else
                    totalCount = _playlists.Count();

                foreach (Playlist pl in _playlists)
                {
                    if (!canExecute()) return false;

                    if (OptionalActionId == 0)
                    {
                        setProgress(progress++, totalCount);
                    }
                    else if (OptionalActionId == 1)
                    {
                        // remove tracks too
                        while (pl.TrackCount > 0)
                        {
                            if (!canExecute()) return false;

                            setProgress(progress++, totalCount);

                            tryDo(() => device.Tracks.Remove(pl[0]), canSkip: true, obj: pl[0]);
                        }
                    }

                    if (OptionalActionId == 0 || (OptionalActionId == 1 && pl.TrackCount == 0))
                    {
                        // remove playlist if it empty
                        if (tryDo(() => device.Playlists.Remove(pl, false), canSkip: true, obj: pl))
                        {
                            notProcessedItems.Remove(pl);
                        }
                    }
                }
            }
            else if (_tracks != null)
            {                
                int progress = 0;
                int totalCount = _tracks.Count();

                foreach (Track tr in _tracks)
                {
                    notProcessedItems.Add(tr);
                }

                foreach (Track tr in _tracks)
                {
                    if (!canExecute()) return false;

                    setProgress(progress++, totalCount);

                    if (OptionalActionId == 0)
                    {
                        // remove track from dev at all
                        if (tryDo(() => device.Tracks.Remove(tr), canSkip: true, obj: tr))
                        {
                            notProcessedItems.Remove(tr);
                        }
                    }
                    else if (OptionalActionId == 1)
                    {
                        // remove track just from playlist
                        if (tryDo(() => _playList.RemoveTrack(tr), canSkip: true, obj: tr))
                        {
                            notProcessedItems.Remove(tr);
                        }
                    }
                }
            }            

            return true;
        }
    }
}
