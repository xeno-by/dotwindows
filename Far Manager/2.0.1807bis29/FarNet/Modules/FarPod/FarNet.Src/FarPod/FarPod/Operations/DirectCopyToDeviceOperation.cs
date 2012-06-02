namespace FarPod.Operations
{
    using System.Collections.Generic;
    using System.Linq;
    using FarNet;
    using FarPod.Explorers;
    using FarPod.Extensions;
    using FarPod.Operations.Bases;
    using FarPod.Resources;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// DirectCopyToDeviceOperation
    /// </summary>
    class DirectCopyToDeviceOperation : OnDeviceOperation
    {
        // can copy files to playlist by name
        // can copy playlists to playlist by name
        // create target playlist if not exist

        private readonly IEnumerable<Playlist> _playlists;
        private readonly IEnumerable<Track> _tracks;

        private readonly FarPodExplorerBase _sourceContext;
        private readonly FarPodExplorerBase _targetContext;

        private readonly string _targetPlayListName;

        private readonly bool _isMove;

        private DirectCopyToDeviceOperation(
            FarPodExplorerBase sourceContext,
            FarPodExplorerBase targetContext,
            string targetPlayListName,
            bool isMove)
            : base(sourceContext.CurrentDevice)
        {
            _sourceContext = sourceContext;
            _targetContext = targetContext;

            _targetPlayListName = targetPlayListName;

            _isMove = isMove;

            isWriteMode = true;

            setText(MsgStr.MsgCopyOrMoveToDevice);
        }

        public DirectCopyToDeviceOperation(
            FarPodExplorerBase sourceContext,
            IEnumerable<Playlist> playlists,
            FarPodExplorerBase targetContext,
            string targetPlayListName,
            bool isMove)
            : this(sourceContext, targetContext, targetPlayListName, isMove)
        {
            _playlists = playlists;
        }

        public DirectCopyToDeviceOperation(
            FarPodExplorerBase sourceContext,
            IEnumerable<Track> tracks,
            FarPodExplorerBase targetContext,
            string targetPlayListName,
            bool isMove)
            : this(sourceContext, targetContext, targetPlayListName, isMove)
        {
            _tracks = tracks;
        }

        protected override bool workOnDevice()
        {
            if (!device.IsEqualToDevice(_targetContext.CurrentDevice))
            {
                Far.Net.Message(MsgStr.MsgDirectOperationOnDiffDeviceWarning, MsgStr.FarPod);

                return false;
            }

            Playlist targetPlaylist = device.Playlists.GetPlaylistByName(_targetPlayListName);

            if (targetPlaylist == null)
            {
                tryDo(() =>
                {
                    targetPlaylist = device.Playlists.Add(_targetPlayListName);
                },
                obj: _targetPlayListName);
            }

            notProcessedItems.Clear();

            if (_playlists != null)
            {
                int progress = 0;
                int totalCount = _playlists.Sum(p => p.TrackCount);

                foreach (Playlist pl in _playlists)
                {
                    notProcessedItems.Add(pl);
                }

                List<Track> trackForRemove = new List<Track>();

                foreach (Playlist pl in _playlists)
                {
                    if (!canExecute()) return false;

                    int copyTrackCount = 0;

                    foreach (Track tr in pl.Tracks)
                    {
                        if (!canExecute()) return false;

                        setProgress(progress++, totalCount);

                        tryDo(() =>
                        {
                            if (targetPlaylist.ContainsTrack(tr) == false)
                            {
                                targetPlaylist.AddTrack(tr);
                            }

                            trackForRemove.Add(tr);

                            copyTrackCount++;
                        },
                        canSkip: true,
                        obj: tr);
                    }

                    // delete original track on isMove == true
                    if (_isMove)
                    {
                        // remove all tracks from original pl                                        
                        while (trackForRemove.Count > 0)
                        {
                            if (!canExecute()) return false;

                            tryDo(() => pl.RemoveTrack(trackForRemove[0]), canSkip: true, obj: trackForRemove[0]);
                        }

                        if (pl.TrackCount == 0)
                        {
                            if (tryDo(() => device.Playlists.Remove(pl, false), canSkip: true, obj: pl))
                            {
                                notProcessedItems.Remove(pl);
                            }
                        }
                    }
                    else if (pl.TrackCount == copyTrackCount)
                    {
                        notProcessedItems.Remove(pl);
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

                    if (tryDo(() =>
                        {
                            if (targetPlaylist.ContainsTrack(tr) == false)
                            {
                                targetPlaylist.AddTrack(tr);
                            }
                        },
                        canSkip: true,
                        obj: tr))
                    {
                        // delete original track on isMove == true
                        if (_isMove)
                        {
                            if (tryDo(() => _sourceContext.CurrentPlayList.RemoveTrack(tr), canSkip: true, obj: tr))
                            {
                                notProcessedItems.Remove(tr);
                            }
                        }
                        else
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
