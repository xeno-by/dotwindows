namespace FarPod.Operations
{
    using FarPod.Operations.Bases;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// CreatePlayListOperation
    /// </summary>
    class CreatePlayListOperation : OnDeviceOperation
    {
        private readonly string _name;
        private readonly PlaylistSortField _sortField;

        public CreatePlayListOperation(IPod dev, string name, PlaylistSortField sortField)
            : base(dev)
        {
            _name = name;
            _sortField = sortField;

            isWriteMode = true;
        }

        protected override bool workOnDevice()
        {
            return tryDo(() =>
            {
                device.Playlists.Add(_name).SortField = _sortField;
            },
            obj: _name);            
        }
    }
}
