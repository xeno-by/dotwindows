namespace FarPod.Explorers
{
    using System;
    using System.Collections.Generic;
    using FarNet;
    using FarPod.Common;
    using FarPod.DataTypes;
    using FarPod.Resources;
    using FarPod.Services;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Base module explorer
    /// </summary>
    abstract class FarPodExplorerBase : Explorer
    {
        protected const string Dots = "..";
        protected const string Backslash = @"\";

        protected EFileSystemLevel currentLevel = EFileSystemLevel.OnRoot;

        protected string deviceName;
        protected string playlistName;

        public IPod CurrentDevice
        {
            get
            {
                if (string.IsNullOrEmpty(deviceName)) return null;

                foreach (IPod dev in FarPodContext.Current.DeviceSource.GetDevices())
                {
                    if (deviceName == getDeviceName(dev)) return dev;
                }

                return null;
            }
        }

        public Playlist CurrentPlayList
        {
            get
            {
                if (string.IsNullOrEmpty(playlistName) || CurrentDevice == null) return null;

                foreach (Playlist pl in CurrentDevice.Playlists)
                {
                    if (playlistName == getPlaylistName(pl)) return pl;
                }

                return null;
            }
        }

        public EFileSystemLevel CurrentLevel
        {
            get
            {
                return currentLevel;
            }
        }

        protected FarPodExplorerBase(Guid typeId)
            : base(typeId)
        {

        }

        public override Panel CreatePanel()
        {
            return new FarPodPanel(this) { Title = MsgStr.FarPodPrefix + Location };
        }

        public virtual void Refresh()
        {

        }

        protected string getDeviceName(IPod dev)
        {
            return dev.DeviceInfo.Family.ToString();
        }

        protected string getPlaylistName(Playlist pl)
        {
            return string.Format(FarPodSetting.Default.PlayListNameFormat, pl.Name, pl.TrackCount);
        }

        protected string getCurrentLocation()
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                return Backslash;
            }
            else if (string.IsNullOrEmpty(playlistName))
            {
                return Backslash + deviceName;
            }
            else
            {
                return Backslash + deviceName + Backslash + playlistName;
            }
        }

        protected FarFile createDirectory(string name, object data)
        {
            FarFile dir = new SetFile();

            dir.Name = name;
            dir.Data = data;
            dir.IsDirectory = true;

            return dir;
        }

        protected FarFile createParentElement()
        {
            FarFile parentLevel = new SetFile();

            parentLevel.IsDirectory = true;
            parentLevel.Name = Dots;

            return parentLevel;
        }

        protected JobResult processResult(OperationResult or, bool isSilent, IList<FarFile> fileToStay = null)
        {
            IList<FarFile> notProcessedFiles = or.ResultData as IList<FarFile>;

            if (notProcessedFiles != null && fileToStay != null)
                foreach (var ff in notProcessedFiles) fileToStay.Add(ff);

            if (!isSilent && !string.IsNullOrEmpty(or.Message))
                Far.Net.Message(or.Message, MsgStr.FarPod, MsgOptions.Warning);

            if (notProcessedFiles != null)
            {
                return JobResult.Incomplete;
            }
            else if (or.IsFailed)
            {
                return JobResult.Ignore;
            }
            else
            {
                return JobResult.Done;
            }
        }

        protected FarPodOperationService get(ExplorerEventArgs e, FarPodExplorerBase expl = null)
        {
            return new FarPodOperationService(expl ?? this, e.Mode);
        }
    }
}
