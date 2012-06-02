namespace FarPod.Explorers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FarNet;
    using FarPod.Common;
    using FarPod.DataTypes;
    using FarPod.Helpers;

    /// <summary>
    /// Track level explorer
    /// </summary>
    class TrackExplorer : FileFarPodExplorerBase
    {
        public TrackExplorer(string dvName, string plName)
            : base(new Guid(GuidContants.TrackExplorerGuid))
        {
            currentLevel = EFileSystemLevel.OnPlayList;

            deviceName = dvName;
            playlistName = plName;

            Location = getCurrentLocation();
        }

        public override IList<FarFile> GetFiles(GetFilesEventArgs args)
        {
            IList<FarFile> realList = new List<FarFile>();

            realList.Add(createParentElement());

            foreach (var track in CurrentPlayList.Tracks)
            {
                FarFile ff = new SetFile();

                ff.Name = IPodTrackFormatter.Get(track, FarPodSetting.Default.TrackNameFormatForPanel, true) + Path.GetExtension(track.FilePath);

                ff.Data = track;

                ff.Length = track.FileSize.ByteCount;

                realList.Add(ff);
            }

            return realList;
        }
    }
}
