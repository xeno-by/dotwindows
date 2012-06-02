namespace FarPod
{
    using System;
    using FarNet;
    using FarPod.DataTypes;
    using FarPod.Explorers;
    using FarPod.Menus;
    using FarPod.Resources;

    /// <summary>
    /// Generic module file panel
    /// </summary>
    class FarPodPanel : Panel
    {
        protected FarPodExplorerBase myExplorer
        {
            get
            {
                return (FarPodExplorerBase)Explorer;
            }
        }

        public FarPodPanel(Explorer explorer)
            : base(explorer)
        {
            Title = MsgStr.FarPodPrefix;
            FormatName = MsgStr.FarPodPrefix;

            Highlighting = PanelHighlighting.Full;            
            DotsMode = PanelDotsMode.Off;

            UseFilter = true;

            //InfoItems = 
            //SetPlan()
            //ViewPlan
        }

        protected bool ensurePathExist()
        {
            bool isChanged = false;

            // if FS.CurrentDevice == null && FS.CurrentLevel == Device goto up
            if (myExplorer.CurrentLevel == EFileSystemLevel.OnPlayList && 
                myExplorer.CurrentPlayList == null)
            {                
                isChanged = true;
            }
            // if FS.CurrentPlaylist == null && FS.CurrentLevel == PlayList goto up
            if (myExplorer.CurrentLevel == EFileSystemLevel.OnDevice && 
                myExplorer.CurrentDevice == null)
            {                
                isChanged = true;
            }

            if (isChanged)
            {
                var parent = (FarPodPanel)Parent; 

                CloseChild();

                parent.ensurePathExist();
            }

            return !isChanged;
        }

        protected bool tryMenuOperation()
        {
            return new FarPodCommandMenu(this, myExplorer).Show();
        }

        protected bool tryRefresh()
        {
            myExplorer.Refresh();

            ProcessUpdatePanel();

            return true;
        }

        public void UpdateAnotherPanelIfNeed()
        {
            if (TargetPanel as FarPodPanel != null)
            {
                ((FarPodPanel)TargetPanel).ProcessUpdatePanel();
            }
        }

        public void ProcessUpdatePanel()
        {
            if (ensurePathExist())
            {
                Update(true);

                Redraw();
            }
        }        

        public override void OnThisFileChanged(EventArgs args)
        {
            base.OnThisFileChanged(args);

            UpdateAnotherPanelIfNeed();            
        }

        public override bool UIKeyPressed(int code, KeyStates state)
        {
            if (state == (KeyStates.Control | KeyStates.Alt))
            {
                /* */
            }
            else if (state == (KeyStates.Control))
            {
                switch (code)
                {
                    case VKeyCode.R:
                        return tryRefresh();
                }
            }
            else if (state == (KeyStates.Shift))
            {                
                switch (code)
                {
                    case VKeyCode.F3:
                        return tryMenuOperation();                        
                }
            }
            else if (state == (KeyStates.None))
            {
                /* */
            }
            
            return base.UIKeyPressed(code, state);
        }

        public override void UIImportFiles(ImportFilesEventArgs args)
        {
            if (!ensurePathExist())
            {             
                return;
            }

            base.UIImportFiles(args);
        }

        public override void UIExportFiles(ExportFilesEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UIExportFiles(args);
        }

        public override void UIDeleteFiles(DeleteFilesEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UIDeleteFiles(args);
        }

        public override void UIAcceptFiles(AcceptFilesEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UIAcceptFiles(args);
        }

        public override void UICreateFile(CreateFileEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UICreateFile(args);
        }

        public override void UIRenameFile(RenameFileEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UIRenameFile(args);
        }

        public override void UICloneFile(CloneFileEventArgs args)
        {
            if (!ensurePathExist())
            {
                return;
            }

            base.UICloneFile(args);
        }        
    }
}
