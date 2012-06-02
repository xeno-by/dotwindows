namespace FarPod.Dialogs.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FarNet;
    using FarPod.Extensions;
    using FarPod.Resources;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Common dialog routines
    /// </summary>
    static class CommonDialog
    {
        public static int ForCopyPlayListsToPath(IEnumerable<Playlist> playlists, ref string destination, bool isMove)
        {
            int totalCount = playlists.Count();

            string op = isMove ? MsgStr.BtnMove : MsgStr.BtnCopy;

            var dlg = new DoubleInputDialog(
                string.Format(MsgStr.MsgCopyPlayLists, op, totalCount),
                MsgStr.MsgOutputFormat,
                MsgStr.FarPod, new[] { op, MsgStr.BtnCancel }, destination, FarPodSetting.Default.TrackNameFormatForCopy);

            dlg.OnInit(d => d.EditControl.IsPath = true);

            dlg.HistoryKey = "CopyToPath";
            dlg.HistoryKey2 = "TrackNameFormatForCopy";

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                destination = dlg.InputText;
                FarPodSetting.Default.TrackNameFormatForCopy = dlg.InputText2;
            }

            return dlg.ClickedButtonNumber;
        }

        public static int ForCopyTracksToPath(IEnumerable<Track> tracks, ref string destination, bool isMove)
        {
            int totalCount = tracks.Count();

            string op = isMove ? MsgStr.BtnMove : MsgStr.BtnCopy;

            var dlg = new DoubleInputDialog(
                string.Format(MsgStr.MsgCopyTracks, op, totalCount),
                MsgStr.MsgOutputFormat,
                MsgStr.FarPod, new[] { op, MsgStr.BtnCancel }, destination, FarPodSetting.Default.TrackNameFormatForCopy);

            dlg.OnInit(d => d.EditControl.IsPath = true);

            dlg.HistoryKey = "CopyToPath";
            dlg.HistoryKey2 = "TrackNameFormatForCopy";

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                destination = dlg.InputText;
                FarPodSetting.Default.TrackNameFormatForCopy = dlg.InputText2;
            }

            return dlg.ClickedButtonNumber;
        }

        public static int ForCopyFilesToDevice(IEnumerable<string> files, ref string toPlayListName, string source, bool isMove)
        {
            int totalCount = files.Count();

            string op = isMove ? MsgStr.BtnMove : MsgStr.BtnCopy;

            var dlg = new InputDialog(
                string.Format(MsgStr.MsgCopyFiles, op, totalCount, toPlayListName),
                MsgStr.FarPod, new[] { op, MsgStr.BtnCancel }, toPlayListName);

            dlg.HistoryKey = "PlaylistName";

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                toPlayListName = dlg.InputText;
            }

            return dlg.ClickedButtonNumber;
        }

        public static int ForDeletePlayLists(IEnumerable<Playlist> playlists, bool isAltMode)
        {
            int totalCount = playlists.Count();

            return Far.Net.Message(
                string.Format(MsgStr.MsgDeletePlayLists, totalCount),
                MsgStr.FarPod, MsgOptions.Warning, new[] { MsgStr.BtnDelete, MsgStr.BtnDeleteWithTracks, MsgStr.BtnCancel });
        }

        public static int ForDeleteTracks(IEnumerable<Track> tracks, bool isAltMode)
        {
            int totalCount = tracks.Count();

            return Far.Net.Message(
                string.Format(MsgStr.MsgDeleteTracks, totalCount),
                MsgStr.FarPod, MsgOptions.Warning, new[] { MsgStr.BtnDelete, MsgStr.BtnDeleteFromPlaylist, MsgStr.BtnCancel });
        }

        public static int ForDirectCopyPlayLists(IEnumerable<Playlist> playlists, ref string toPlayListName, bool isMove)
        {
            int totalCount = playlists.Count();

            string op = isMove ? MsgStr.BtnMove : MsgStr.BtnCopy;

            var dlg = new InputDialog(
                string.Format(MsgStr.MsgCopyPlayLists, op, totalCount),
                MsgStr.FarPod, new[] { op, MsgStr.BtnCancel }, toPlayListName);

            dlg.HistoryKey = "PlaylistName";

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                toPlayListName = dlg.InputText;
            }

            return dlg.ClickedButtonNumber;
        }

        public static int ForDirectCopyTracks(IEnumerable<Track> tracks, ref string toPlayListName, bool isMove)
        {
            int totalCount = tracks.Count();

            string op = isMove ? MsgStr.BtnMove : MsgStr.BtnCopy;

            var dlg = new InputDialog(
                string.Format(MsgStr.MsgCopyTracks, op, totalCount),
                MsgStr.FarPod, new[] { op, MsgStr.BtnCancel }, toPlayListName);

            dlg.HistoryKey = "PlaylistName";

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                toPlayListName = dlg.InputText;
            }

            return dlg.ClickedButtonNumber;
        }

        public static int ForGetPlayListParams(ref string name, ref PlaylistSortField sortField, bool? isMove)
        {
            string message = MsgStr.MsgNewPlayList;

            if (isMove != null)
            {
                message = isMove.Value ? MsgStr.MsgRenamePlayList : MsgStr.MsgLocalCopyPlayList;
            }

            var dlg = new PlaylistParamDialog(
                message,
                MsgStr.FarPod, new[] { isMove == null ? MsgStr.BtnCreate : MsgStr.BtnOk, MsgStr.BtnCancel });

            dlg.PlaylistName = name;
            dlg.PlaylistSortField = sortField.ToString();

            if (dlg.Show() && dlg.ClickedButtonNumber == 0)
            {
                name = dlg.PlaylistName;
                sortField = (PlaylistSortField)Enum.Parse(typeof(PlaylistSortField), dlg.PlaylistSortField);
            }

            return dlg.ClickedButtonNumber;
        }
    }
}
