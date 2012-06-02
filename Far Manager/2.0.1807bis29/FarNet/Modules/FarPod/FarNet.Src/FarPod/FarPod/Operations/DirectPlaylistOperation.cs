namespace FarPod.Operations
{
    using FarPod.Operations.Bases;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// DirectLocalCopyOperation
    /// </summary>
    class DirectPlaylistOperation : OnDeviceOperation
    {
        // can rename playlist
        // can local copy playlist with new name

        private readonly Playlist _playList;
        private readonly string _newName;
        private readonly PlaylistSortField _newSortField;
        private readonly bool _isMove;

        public DirectPlaylistOperation(IPod dev, Playlist pl, string newName, PlaylistSortField sortField, bool isMove)
            : base(dev)
        {
            _playList = pl;
            _newName = newName;
            _newSortField = sortField;
            _isMove = isMove;

            isWriteMode = true;
        }

        protected override bool workOnDevice()
        {
            if (_playList != null)
            {
                if (_isMove)
                {
                    // rename pl
                    tryDo(() =>
                    {
                        _playList.Name = _newName;
                        _playList.SortField = _newSortField;
                    },
                    canSkip: true,
                    obj: _newName);
                }
                else
                {
                    // copy to new pl
                    Playlist targetPlaylist = device.Playlists.GetPlaylistByName(_newName);

                    if (targetPlaylist == null)
                    {
                        tryDo(() =>
                        {
                            targetPlaylist = device.Playlists.Add(_newName);
                            targetPlaylist.SortField = _newSortField;
                        },
                        obj: _newName);
                    }

                    foreach (Track track in _playList.Tracks)
                    {
                        if (targetPlaylist.ContainsTrack(track) == false)
                        {
                            tryDo(() =>
                            {
                                targetPlaylist.AddTrack(track);
                            },
                            canSkip: true,
                            obj: track);
                        }
                    }
                }
            }

            return true;   
        }
    }
}
