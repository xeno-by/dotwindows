namespace FarPod.Explorers
{
    using System;
    using System.Collections.Generic;
    using FarNet;
    using FarPod.Common;
    using FarPod.DataTypes;
    using FarPod.Extensions;

    /// <summary>
    /// Playlist level explorer
    /// </summary>
    class PlayListExplorer : FileFarPodExplorerBase
    {
        public PlayListExplorer(string dvName)
            : base(new Guid(GuidContants.PlayListExplorerGuid))
        {
            currentLevel = EFileSystemLevel.OnDevice;

            deviceName = dvName;
            playlistName = string.Empty;

            Location = getCurrentLocation();

            Functions |=
                ExplorerFunctions.CreateFile |
                ExplorerFunctions.RenameFile |
                ExplorerFunctions.CloneFile;
        }

        public override IList<FarFile> GetFiles(GetFilesEventArgs args)
        {
            IList<FarFile> realList = new List<FarFile>();

            realList.Add(createParentElement());

            foreach (var playList in CurrentDevice.Playlists)
            {
                realList.Add(createDirectory(getPlaylistName(playList), playList));
            }

            return realList;
        }

        public override Explorer ExploreDirectory(ExploreDirectoryEventArgs args)
        {
            return new TrackExplorer(deviceName, args.File.Name);
        }

        public override void CreateFile(CreateFileEventArgs args)
        {
            OperationResult or = get(args).CreatePlayList(string.Empty);

            args.PostName = (string)or.ResultData ?? string.Empty;

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent));
        }

        public override void RenameFile(RenameFileEventArgs args)
        {
            OperationResult or = get(args).DirectLocalCopyOrRename(args.File, true);

            args.PostName = (string)or.ResultData ?? string.Empty;

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent));
        }

        public override void CloneFile(CloneFileEventArgs args)
        {
            OperationResult or = get(args).DirectLocalCopyOrRename(args.File, false);

            args.PostName = (string)or.ResultData ?? string.Empty;

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent));
        }
    }
}
