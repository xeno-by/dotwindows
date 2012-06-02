namespace FarPod.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using FarNet;
    using FarPod.DataTypes;
    using FarPod.Dialogs.Common;
    using FarPod.Explorers;
    using FarPod.Operations;
    using FarPod.Operations.Bases;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Main operation service
    /// </summary>
    class FarPodOperationService
    {
        private readonly FarPodExplorerBase _fs;
        private readonly ExplorerModes _operationModes;

        public FarPodOperationService(FarPodExplorerBase fs, ExplorerModes om)
        {
            // OPM_FIND or OPM_EDIT or OPM_VIEW or OPM_QUICKVIEW - get one file for view
            // OPM_SILENT with no message

            _fs = fs;
            _operationModes = om;
        }

        public OperationResult CopyOrMoveContentFromSelf(IList<FarFile> files, string destination, bool isMove)
        {
            OnDeviceOperation doc = null;

            if (_fs.CurrentLevel == EFileSystemLevel.OnDevice)
            {
                var playLists = files.Select(p => (Playlist)p.Data);

                if (IsSilentOperation() ||
                    CommonDialog.ForCopyPlayListsToPath(playLists, ref destination, isMove) == 0)
                {
                    doc = new CopyContentFromSelfOperation(
                        _fs.CurrentDevice, playLists, destination, isMove);

                }
            }
            else if (_fs.CurrentLevel == EFileSystemLevel.OnPlayList)
            {
                var tracks = files.Select(p => (Track)p.Data);

                if (IsSilentOperation() ||
                    CommonDialog.ForCopyTracksToPath(tracks, ref destination, isMove) == 0)
                {
                    doc = new CopyContentFromSelfOperation(
                         _fs.CurrentDevice, tracks, destination, isMove, IsInternalOperation() || IsSilentOperation());
                }
            }

            return execOperation(doc, files);
        }

        public OperationResult CopyOrMoveContentToSelf(IList<FarFile> files, string source, bool isMove)
        {
            OnDeviceOperation doc = null;

            var filesNames = files.Select(p => p.Name);

            string playListName = _fs.CurrentPlayList != null ?
                _fs.CurrentPlayList.Name : string.Empty;

            if (IsSilentOperation() ||
                CommonDialog.ForCopyFilesToDevice(filesNames, ref playListName, source, isMove) == 0)
            {
                doc = new CopyContentToSelfOperation(
                    _fs.CurrentDevice, filesNames, source, playListName, isMove);
            }

            return execOperation(doc, files);
        }

        public OperationResult DeleteContent(IList<FarFile> files, bool isAltMode)
        {
            // IsSilentOperation() ||  ???

            OnDeviceOperation doc = null;

            if (_fs.CurrentLevel == EFileSystemLevel.OnDevice)
            {
                var playLists = files.Select(p => (Playlist)p.Data);

                int actionId = CommonDialog.ForDeletePlayLists(playLists, isAltMode);

                if (actionId == 0 ||
                    actionId == 1)
                {
                    doc = new DeleteContentOperation(
                         _fs.CurrentDevice, playLists, isAltMode)
                        {
                            OptionalActionId = actionId
                        };
                }
            }
            else if (_fs.CurrentLevel == EFileSystemLevel.OnPlayList)
            {
                var tracks = files.Select(p => (Track)p.Data);

                int actionId = CommonDialog.ForDeleteTracks(tracks, isAltMode);

                if (actionId == 0 ||
                    actionId == 1)
                {
                    doc = new DeleteContentOperation(
                         _fs.CurrentDevice, _fs.CurrentPlayList, tracks, isAltMode)
                        {
                            OptionalActionId = actionId
                        };
                }
            }

            return execOperation(doc, files);
        }

        public OperationResult CreatePlayList(string name)
        {
            OnDeviceOperation doc = null;

            PlaylistSortField sortField = PlaylistSortField.Unknown;

            if (string.IsNullOrEmpty(name))
                name = FarPodSetting.Default.NewPlayListName;

            if (CommonDialog.ForGetPlayListParams(ref name, ref sortField, null) == 0)
            {
                doc = new CreatePlayListOperation(
                     _fs.CurrentDevice, name, sortField);
            }

            return execOperation(doc, null).SetResult(name);
        }

        public OperationResult DirectCopyOrMoveToDevice(IList<FarFile> files, FarPodExplorerBase anotherController, bool isMove)
        {
            // IsSilentOperation() || ???

            OnDeviceOperation doc = null;

            string playListName = anotherController.CurrentPlayList != null ?
                anotherController.CurrentPlayList.Name : string.Empty;

            if (_fs.CurrentLevel == EFileSystemLevel.OnDevice)
            {
                // copy selected playlists
                var playLists = files.Select(p => (Playlist)p.Data);

                if (CommonDialog.ForDirectCopyPlayLists(playLists, ref playListName, isMove) == 0)
                {
                    doc = new DirectCopyToDeviceOperation(
                         _fs, playLists, anotherController, playListName, isMove);
                }
            }
            else if (_fs.CurrentLevel == EFileSystemLevel.OnPlayList)
            {
                // copy selected tracks
                var tracks = files.Select(p => (Track)p.Data);

                if (CommonDialog.ForDirectCopyTracks(tracks, ref playListName, isMove) == 0)
                {
                    doc = new DirectCopyToDeviceOperation(
                        _fs, tracks, anotherController, playListName, isMove);
                }
            }

            return execOperation(doc, files);
        }

        public OperationResult DirectLocalCopyOrRename(FarFile currentFile, bool isMove)
        {
            // IsSilentOperation() || ???

            OnDeviceOperation doc = null;

            Playlist pl = (Playlist)currentFile.Data;

            string name = pl.Name;

            PlaylistSortField sortField = pl.SortField;

            if (CommonDialog.ForGetPlayListParams(ref name, ref sortField, isMove) == 0)
            {
                doc = new DirectPlaylistOperation(
                    _fs.CurrentDevice, pl, name, sortField, isMove);
            }

            return execOperation(doc, null).SetResult(name);
        }

        public bool IsSilentOperation()
        {
            // OPM_FIND or OPM_EDIT or OPM_VIEW or OPM_QUICKVIEW - get one file for view
            // OPM_SILENT with no message

            return (_operationModes & ExplorerModes.Silent) == ExplorerModes.Silent;
        }

        public bool IsInternalOperation()
        {
            // OPM_FIND or OPM_EDIT or OPM_VIEW or OPM_QUICKVIEW - get one file for view
            // OPM_SILENT with no message

            return
                (_operationModes & ExplorerModes.Find) == ExplorerModes.Find ||
                (_operationModes & ExplorerModes.Edit) == ExplorerModes.Edit ||
                (_operationModes & ExplorerModes.View) == ExplorerModes.View ||
                (_operationModes & ExplorerModes.QuickView) == ExplorerModes.QuickView;
        }

        private OperationResult execOperation(OnDeviceOperation oc, IEnumerable<FarFile> inputFileList)
        {
            if (oc != null)
            {
                oc.Execute();

                var result = oc.OperationResult;

                var notProcessedFiles = new List<FarFile>();

                if (inputFileList != null && oc.NotProcessedItems != null)
                {
                    foreach (object dataItem in oc.NotProcessedItems)
                    {
                        FarFile ff = inputFileList.FirstOrDefault(p => p.Data == dataItem);

                        if (ff == null && dataItem is string) ff = inputFileList.FirstOrDefault(p => p.Name == (string)dataItem);

                        if (ff != null) notProcessedFiles.Add(ff);
                    }

                    if (notProcessedFiles.Count > 0) result.SetResult(notProcessedFiles);
                }

                return result;
            }
            else
            {
                return new OperationResult(true);
            }
        }
    }
}
