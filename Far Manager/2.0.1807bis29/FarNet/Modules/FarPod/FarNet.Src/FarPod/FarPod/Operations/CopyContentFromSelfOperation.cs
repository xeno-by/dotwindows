namespace FarPod.Operations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FarNet;
    using FarPod.DataTypes;
    using FarPod.Dialogs;
    using FarPod.Helpers;
    using FarPod.Operations.Bases;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// CopyContentFromSelfOperation
    /// </summary>
    class CopyContentFromSelfOperation : CopyContentOperation
    {
        private readonly IEnumerable<Playlist> _playlists;
        private readonly IEnumerable<Track> _tracks;

        private readonly string _destination;
        private readonly bool _directCopy;

        // не лучшее решение (
        private string _originalFileName;
        private string _newFileName;

        private CopyContentFromSelfOperation(IPod dev, string destination, bool isMove)
            : base(dev)
        {
            _destination = destination;

            isMoveOperation = isMove;           
            isWriteMode = isMove;

            setText(MsgStr.MsgCopyOrMoveToPath);
        }

        public CopyContentFromSelfOperation(IPod dev, IEnumerable<Playlist> playlists, string destination, bool isMove)
            : this(dev, destination, isMove)
        {
            _playlists = playlists;
        }

        public CopyContentFromSelfOperation(IPod dev, IEnumerable<Track> tracks, string destination, bool isMove, bool directCopy)
            : this(dev, destination, isMove)
        {
            _tracks = tracks;

            _directCopy = directCopy;
        }

        protected override bool workOnDevice()
        {
            if (!Directory.Exists(_destination))
                tryDo(() => Directory.CreateDirectory(_destination), obj: _destination);

            var forDeleteTrackList = new List<Track>();

            notProcessedItems.Clear();

            if (_playlists != null)
            {
                int progress = 0;
                int totalCount = _playlists.Sum(p => p.TrackCount);                

                foreach (Playlist pl in _playlists)
                {
                    notProcessedItems.Add(pl);
                }

                totalByteToCopy = _playlists
                    .SelectMany(p => p.Tracks)
                    .Sum(t => t.FileSize.ByteCount);

                foreach (Playlist pl in _playlists)
                {
                    string playlistPath = Path.Combine(_destination, pl.Name);

                    int copyTrackCount = 0;

                    foreach (Track tr in pl.Tracks)
                    {
                        if (!canExecute()) return false;

                        setProgress(progress++, totalCount);

                        if (copyTrackTo(tr, playlistPath))
                        {
                            if (isMoveOperation) forDeleteTrackList.Add(tr);

                            copyTrackCount++;
                        }
                    }

                    if (!isMoveOperation && copyTrackCount == pl.TrackCount) notProcessedItems.Remove(pl);
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

                totalByteToCopy = _tracks.Sum(t => t.FileSize.ByteCount);

                foreach (Track tr in _tracks)
                {
                    if (!canExecute()) return false;

                    setProgress(progress++, totalCount);

                    if (copyTrackTo(tr, _destination))
                    {
                        if (isMoveOperation)
                            forDeleteTrackList.Add(tr);
                        else
                            notProcessedItems.Remove(tr);
                    }                                            
                }
            }

            if (forDeleteTrackList.Count > 0)
            {
                foreach (Track tr in forDeleteTrackList)
                {
                    if (!canExecute()) return false;

                    if (tryDo(() => device.Tracks.Remove(tr), canSkip: true, obj: tr))
                    {
                        notProcessedItems.Remove(tr);
                    }
                }

                if (_playlists != null)
                {
                    foreach (Playlist pl in _playlists)
                    {
                        if (!canExecute()) return false;

                        if (tryDo(() =>
                            {
                                if (pl.TrackCount == 0) device.Playlists.Remove(pl, false);
                            },
                            canSkip: true,
                            obj: pl))
                        {
                            notProcessedItems.Remove(pl);
                        }
                    }
                }
            }

            return true;
        }

        private bool copyTrackTo(Track tr, string path)
        {
            _originalFileName = Path.Combine(
                device.FileSystem.DriveLetter,
                tr.FilePath);

            _newFileName = Path.Combine(
                path,
                IPodTrackFormatter.Get(tr,
                    _directCopy ? FarPodSetting.Default.TrackNameFormatForPanel : FarPodSetting.Default.TrackNameFormatForCopy, true)
                     + Path.GetExtension(tr.FilePath));

            if (ifTargetExist()) return false;

            string targetDir = Path.GetDirectoryName(_newFileName);

            if (Directory.Exists(targetDir) == false)
                tryDo(() => Directory.CreateDirectory(targetDir), obj: targetDir);

            return copyFile(_originalFileName, _newFileName) && !IsCanceled;
        }

        protected override CollisionBehaviorDialog getCollisionBehaviorDialog()
        {
            return CollisionBehaviorDialog.ForFile(_newFileName);
        }

        protected override bool processExist(Func<EExistCollisionStrategy> getBehavior)
        {
            bool isTryCopy = true;

            while (File.Exists(_newFileName) && isTryCopy && !IsCanceled)
            {
                EExistCollisionStrategy currentBehavior = _directCopy ?
                    EExistCollisionStrategy.Replace : getBehavior();

                try
                {
                    if (currentBehavior == EExistCollisionStrategy.Break)
                    {
                        IsCanceled = true;

                        isTryCopy = false;
                    }
                    else if (currentBehavior == EExistCollisionStrategy.Skip)
                    {
                        isTryCopy = false;
                    }
                    else if (currentBehavior == EExistCollisionStrategy.Rename)
                    {
                        pauseTimer();

                        var dlg = new InputDialog(
                            MsgStr.MsgNewFileList,
                            MsgStr.FarPod, new [] { MsgStr.BtnRename, MsgStr.BtnCancel }, _newFileName);

                        dlg.HistoryKey = "NewFileName";

                        if (dlg.Show() && dlg.ClickedButtonNumber == 0)
                        {
                            // ask for new name
                            _newFileName = dlg.InputText;
                        }

                        resumeTimer();
                    }
                    else if (currentBehavior == EExistCollisionStrategy.Replace)
                    {
                        File.SetAttributes(_newFileName, FileAttributes.Normal);
                        File.Delete(_newFileName);
                    }
                }
                catch (Exception e)
                {
                    pauseTimer();

                    Far.Net.ShowError(MsgStr.FarPod, e);

                    collisionBehavior = EExistCollisionStrategy.None;

                    resumeTimer();
                }
            }

            return isTryCopy;
        }
    }
}
