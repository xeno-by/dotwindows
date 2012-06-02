namespace FarPod.Explorers
{
    using System;
    using System.Collections.Generic;
    using FarNet;
    using FarPod.Common;
    using FarPod.DataTypes;

    /// <summary>
    /// Device level explorer
    /// </summary>
    class DeviceExplorer : FarPodExplorerBase
    {
        public DeviceExplorer()
            : base(new Guid(GuidContants.DeviceExplorerGuid))
        {
            currentLevel = EFileSystemLevel.OnRoot;

            deviceName = string.Empty;
            playlistName = string.Empty;

            Location = getCurrentLocation();

            Functions =
                ExplorerFunctions.None;
        }

        public override IList<FarFile> GetFiles(GetFilesEventArgs args)
        {
            IList<FarFile> realList = new List<FarFile>();

            realList.Add(createParentElement());

            foreach (var dev in FarPodContext.Current.DeviceSource.GetDevices())
            {
                realList.Add(createDirectory(getDeviceName(dev), dev));
            }

            return realList;
        }

        public override Explorer ExploreDirectory(ExploreDirectoryEventArgs args)
        {
            return new PlayListExplorer(args.File.Name);
        }

        public override void Refresh()
        {
            FarPodContext.Current.DeviceSource.Refresh();
        }
    }
}
