namespace FarPod.Menus
{
    using System.Collections.Generic;
    using System.Linq;
    using FarNet;
    using FarPod.Common;
    using FarPod.DataTypes;
    using FarPod.Dialogs;
    using FarPod.Dialogs.Bases;
    using FarPod.Explorers;
    using FarPod.Resources;
    using SharePodLib;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Module Main menu service
    /// </summary>
    class FarPodCommandMenu
    {
        enum EFarPodCommand
        {
            // 0*** - common 
            // 1*** - device 
            // 2*** - playlist 
            // 3*** - track

            ClosePanel = 0001,
            EjectDevice = 1001,
            TrackEditTags = 3001,
            BatchEditTags = 3002,
        }

        private readonly FarPodPanel _panel;
        private readonly FarPodExplorerBase _controller;

        public FarPodCommandMenu(FarPodPanel panel, FarPodExplorerBase controller)
        {
            _panel = panel;
            _controller = controller;
        }

        public bool Show()
        {
            EFarPodCommand cmd = createAndShowMenu();

            if (cmd > 0) processCommand(cmd);

            return cmd >= 0;
        }

        private EFarPodCommand createAndShowMenu()
        {
            IMenu commandMenu = Far.Net.CreateMenu();

            // add common items on top                                    

            switch (_controller.CurrentLevel)
            {
                case EFileSystemLevel.OnRoot:
                    {                        
                        commandMenu.Add(MsgStr.MsgMenuEjectDevice).Data = EFarPodCommand.EjectDevice;                        
                    }
                    break;
                case EFileSystemLevel.OnDevice:
                    {
                        commandMenu.Add(MsgStr.MsgMenuEditTrackTags).Data = EFarPodCommand.TrackEditTags;
                        commandMenu.Add(MsgStr.MsgMenuBatchTrackTags).Data = EFarPodCommand.BatchEditTags;                        
                    }
                    break;
                case EFileSystemLevel.OnPlayList:
                    {
                        commandMenu.Add(MsgStr.MsgMenuEditTrackTags).Data = EFarPodCommand.TrackEditTags;
                        commandMenu.Add(MsgStr.MsgMenuBatchTrackTags).Data = EFarPodCommand.BatchEditTags;                        
                    }
                    break;
            }

            // add common items on bottom                        

            commandMenu.Add(string.Empty).IsSeparator = true;

            commandMenu.Add(MsgStr.MsgMenuClosePanel).Data = EFarPodCommand.ClosePanel;

            commandMenu.Show();

            return (EFarPodCommand)(commandMenu.SelectedData ?? 0);
        }

        private void processCommand(EFarPodCommand cmd)
        {
            FarFile currentDataItem = _panel.CurrentFile;

            switch (cmd)
            {
                case EFarPodCommand.EjectDevice:
                    {
                        FarPodContext.Current.DeviceSource.Eject((IPod)currentDataItem.Data);

                        updatePanel();
                    }
                    break;
                case EFarPodCommand.ClosePanel:
                    {
                        if (_panel != null) _panel.Close();
                    }
                    break;
                case EFarPodCommand.TrackEditTags:
                case EFarPodCommand.BatchEditTags:
                    {
                        List<Track> trackList = null;

                        if (_controller.CurrentLevel == EFileSystemLevel.OnDevice)
                            trackList = _panel.SelectedList.Select(p => (Playlist)p.Data).SelectMany(v => v.Tracks).ToList();
                        else if (_controller.CurrentLevel == EFileSystemLevel.OnPlayList)
                            trackList = _panel.SelectedList.Select(t => (Track)t.Data).ToList();

                        BaseView dlg = null;

                        if (cmd == EFarPodCommand.TrackEditTags)
                            dlg = new TrackTagEditDialog(_controller.CurrentDevice, trackList);
                        else if (cmd == EFarPodCommand.BatchEditTags)
                            dlg = new BatchTagEditDialog(_controller.CurrentDevice, trackList);                        

                        using (dlg) 
                        {
                            if (dlg.Show())
                            {
                                _panel.UnselectAll();

                                updatePanel();
                            }
                        }                        
                    }                    
                    break;
            }
        }

        private void updatePanel()
        {
            _panel.ProcessUpdatePanel();
            _panel.UpdateAnotherPanelIfNeed();
        }
    }
}
