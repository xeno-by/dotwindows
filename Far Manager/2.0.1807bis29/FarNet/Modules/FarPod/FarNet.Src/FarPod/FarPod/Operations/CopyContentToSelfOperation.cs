namespace FarPod.Operations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FarNet;
    using FarPod.DataTypes;
    using FarPod.Dialogs;
    using FarPod.Extensions;
    using FarPod.Helpers;
    using FarPod.Operations.Bases;
    using FarPod.Resources;
    using FarPod.Services;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// CopyContentToSelfOperation
    /// </summary>
    class CopyContentToSelfOperation : CopyContentOperation
    {
        private readonly IEnumerable<string> _files;
        private readonly string _source;
        private readonly string _targetPlayListName;

        private IPodTrackFactory _trackFactory;

        // не лучшее решение (
        private Playlist _targetPlaylist;
        private NewTrack _newTrack;

        public CopyContentToSelfOperation(IPod dev, IEnumerable<string> files, string source, string targetPlayName, bool isMove)
            : base(dev)
        {
            _files = files;
            _source = source;
            _targetPlayListName = targetPlayName;

            isMoveOperation = isMove;
            isWriteMode = true;

            setText(MsgStr.MsgCopyOrMoveToDevice);
        }

        protected override bool workOnDevice()
        {
            bool isMetaPlayListName = IPodTrackFormatter.HasMetaInfo(_targetPlayListName);

            if (!isMetaPlayListName) createPlayList(_targetPlayListName);

            _trackFactory = new IPodTrackFactory();

            notProcessedItems.Clear();

            // add notProcessedItems logic

            var fse = new FileSystemEnumerator(_source, _files);

            var copyList = new List<string>();

            ProgressFormHelper.Invoke(f =>
            {
                copyList = fse.FetchFiles().ToList();
            },
            MsgStr.MsgGettingFiles);

            // critical !!!
            if (ProgressFormHelper.LastError != null)
                throw ProgressFormHelper.LastError;

            int progress = 0;
            int totalCount = copyList.Count;

            totalByteToCopy = fse.TotalFileSize;            

            foreach (string fileName in copyList)
            {
                if (!canExecute()) return false;

                setProgress(progress++, totalCount);

                if (!tryDo(() =>
                    {
                        _newTrack = _trackFactory.Get(fileName);
                    },
                    canSkip: true,
                    obj: fileName)) continue;

                if (_newTrack == null) continue;

                if (isMetaPlayListName) createPlayList(IPodTrackFormatter.Get(_newTrack, _targetPlayListName));

                if (ifTargetExist()) continue;

                // copy track local before adding               
                string newFileName = Path.Combine(
                    device.FileSystem.IPodControlPath,
                    Path.GetRandomFileName() + Path.GetExtension(fileName));

                if (!copyFile(fileName, newFileName)) continue;

                if (!canExecute()) return false;

                // why ???
                //_newTrack.FilePath = newFileName;

                Track addedTrack = null;

                // try add track to device
                if (tryDo(() =>
                    {
                        addedTrack = device.Tracks.Add(_newTrack);
                    },
                    canSkip: true,
                    obj: _newTrack))
                {
                    // if added
                    if (!_targetPlaylist.IsMaster)
                    {
                        tryDo(() => _targetPlaylist.AddTrack(addedTrack), canSkip: true, obj: addedTrack);
                    }

                    if (isMoveOperation)
                    {
                        // remove original file on move
                        tryDo(() =>
                        {
                            File.SetAttributes(_newTrack.FilePath, FileAttributes.Normal);
                            File.Delete(_newTrack.FilePath);
                        },
                        canSkip: true,
                        obj: _newTrack.FilePath);
                    }
                }
                else
                {
                    // if not remove tmp-copy file
                    if (File.Exists(newFileName))
                    {
                        tryDo(() =>
                        {
                            File.SetAttributes(newFileName, FileAttributes.Normal);
                            File.Delete(newFileName);
                        },
                        canSkip: true,
                        obj: newFileName);
                    }
                }
            }

            if (isMoveOperation)
            {
                // remvoe dirs on move file
                foreach (string dir in fse.GetBypassDirectores())
                {
                    if (!canExecute()) return false;

                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        tryDo(() => Directory.Delete(dir), canSkip: true, obj: dir);
                    }
                }
            }

            return true;
        }

        private void createPlayList(string name)
        {
            _targetPlaylist = device.Playlists.GetPlaylistByName(name);

            if (_targetPlaylist == null)
            {
                tryDo(() =>
                {
                    _targetPlaylist = device.Playlists.Add(name);
                },
                obj: name);
            }
        }

        protected override void finish()
        {
            base.finish();

            if (_trackFactory != null)
            {
                _trackFactory.Dispose();
                _trackFactory = null;
            }
        }

        protected override CollisionBehaviorDialog getCollisionBehaviorDialog()
        {
            return CollisionBehaviorDialog.ForTrack(
                IPodTrackFormatter.Get(_newTrack, FarPodSetting.Default.TrackNameFormatForPanel) + Path.GetExtension(_newTrack.FilePath));
        }

        protected override bool processExist(Func<EExistCollisionStrategy> getBehavior)
        {
            bool isTryCopy = true;

            // exist track check
            Track existTrack = null;

            while ((existTrack = device.GetTrackIfExists(_newTrack)) != null && isTryCopy && !IsCanceled)
            {
                EExistCollisionStrategy currentBehavior = getBehavior();

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
                    else if (currentBehavior == EExistCollisionStrategy.Replace)
                    {
                        device.Tracks.Remove(existTrack);
                    }
                    else if (currentBehavior == EExistCollisionStrategy.AddExistToPlayList)
                    {
                        if (!_targetPlaylist.IsMaster) _targetPlaylist.AddTrack(existTrack);                        

                        isTryCopy = false;
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
