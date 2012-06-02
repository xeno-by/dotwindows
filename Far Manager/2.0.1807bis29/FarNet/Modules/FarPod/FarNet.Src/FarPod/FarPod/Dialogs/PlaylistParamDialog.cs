namespace FarPod.Dialogs
{
    using System;
    using FarPod.Dialogs.Bases;
    using SharePodLib.Parsers.iTunesDB;

    /// <summary>
    /// Playlist parameters dialog impl
    /// </summary>
    class PlaylistParamDialog : BaseActionDialog
    {        
        public string PlaylistName
        {
            get;
            set;
        }

        public string[] PlaylistSortFields
        {
            get;
            private set;
        }

        public string PlaylistSortField
        {
            get;
            set;
        }

        public PlaylistParamDialog(string text, string title, string[] buttons)
            : base(title, text, buttons)
        {            
            PlaylistSortFields = Enum.GetNames(typeof(PlaylistSortField));
        }    
    }
}
