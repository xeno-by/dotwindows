
#pragma warning disable 0649

namespace FarPod.Resources
{
	using System;
	using System.Reflection;
	using FarNet;

    static class MsgStr
    {
		#region
		
		// # Common
					
		/// <summary>
		/// FarPod
		/// </summary>
		public static string FarPod="FarPod"; 
					
		/// <summary>
		/// FarPod:
		/// </summary>
		public static string FarPodPrefix="FarPod:"; 
					
		/// <summary>
		/// GB
		/// </summary>
		public static string GB="GB"; 
					
		/// <summary>
		/// MB
		/// </summary>
		public static string MB="MB"; 
					
		/// <summary>
		/// Unknown
		/// </summary>
		public static string Unknown="Unknown"; 
		
		// # Buttons
					
		/// <summary>
		/// &amp;Ok
		/// </summary>
		public static string BtnOk="&Ok"; 
					
		/// <summary>
		/// &amp;Move
		/// </summary>
		public static string BtnMove="&Move"; 
					
		/// <summary>
		/// &amp;Copy
		/// </summary>
		public static string BtnCopy="&Copy"; 
					
		/// <summary>
		/// Cr&amp;eate
		/// </summary>
		public static string BtnCreate="Cr&eate"; 
					
		/// <summary>
		/// Ca&amp;ncel
		/// </summary>
		public static string BtnCancel="Ca&ncel"; 
					
		/// <summary>
		/// &amp;Skip
		/// </summary>
		public static string BtnSkip="&Skip"; 
					
		/// <summary>
		/// Skip all
		/// </summary>
		public static string BtnSkipAll="Skip all"; 
					
		/// <summary>
		/// Replace
		/// </summary>
		public static string BtnReplace="Replace"; 
					
		/// <summary>
		/// Replace all
		/// </summary>
		public static string BtnReplaceAll="Replace all"; 
					
		/// <summary>
		/// &amp;Retry
		/// </summary>
		public static string BtnRetry="&Retry"; 
					
		/// <summary>
		/// &amp;Delete
		/// </summary>
		public static string BtnDelete="&Delete"; 
					
		/// <summary>
		/// Delete with &amp;tracks
		/// </summary>
		public static string BtnDeleteWithTracks="Delete with &tracks"; 
					
		/// <summary>
		/// Remove from &amp;playlist
		/// </summary>
		public static string BtnDeleteFromPlaylist="Remove from &playlist"; 
					
		/// <summary>
		/// Rena&amp;me
		/// </summary>
		public static string BtnRename="Rena&me"; 
					
		/// <summary>
		/// &amp;Add to playList
		/// </summary>
		public static string BtnAddExistToPlayList="&Add to playList"; 
		
		// # Messages
					
		/// <summary>
		/// Getting devices...
		/// </summary>
		public static string MsgGettingDevices="Getting devices..."; 
					
		/// <summary>
		/// Ejecting ...
		/// </summary>
		public static string MsgEjecting="Ejecting ..."; 
					
		/// <summary>
		/// Getting media files...
		/// </summary>
		public static string MsgGettingFiles="Getting media files..."; 
					
		/// <summary>
		/// Direct copy to diff device not implemented!
		/// </summary>
		public static string MsgNoDirectCopyToDiffDevice="Direct copy to diff device not implemented!"; 
					
		/// <summary>
		/// Copy or Move to path operation ...
		/// </summary>
		public static string MsgCopyOrMoveToPath="Copy or Move to path operation ..."; 
					
		/// <summary>
		/// Copy or Move to device operation ...
		/// </summary>
		public static string MsgCopyOrMoveToDevice="Copy or Move to device operation ..."; 
					
		/// <summary>
		/// Delete operation ... 
		/// </summary>
		public static string MsgDelete="Delete operation ... "; 
					
		/// <summary>
		/// Do you really want to cancel it?
		/// </summary>
		public static string MsgCancel="Do you really want to cancel it?"; 
					
		/// <summary>
		/// Output Name format:
		/// </summary>
		public static string MsgOutputFormat="Output Name format:"; 
					
		/// <summary>
		/// Track already exists
		/// </summary>
		public static string MsgTrackExist="Track already exists"; 
					
		/// <summary>
		/// File already exists
		/// </summary>
		public static string MsgFileExist="File already exists"; 
					
		/// <summary>
		/// Source: {0}
		/// </summary>
		public static string MsgCopierSource="Source: {0}"; 
					
		/// <summary>
		/// Destination: {0}
		/// </summary>
		public static string MsgCopierDestination="Destination: {0}"; 
					
		/// <summary>
		/// Remember choice
		/// </summary>
		public static string MsgRememberChoice="Remember choice"; 
					
		/// <summary>
		/// Playlist sort field:
		/// </summary>
		public static string MsgPlaylistSortField="Playlist sort field:"; 
					
		/// <summary>
		/// &amp;Edit track(s) tags
		/// </summary>
		public static string MsgMenuEditTrackTags="&Edit track(s) tags"; 
					
		/// <summary>
		/// &amp;Batch edit tags
		/// </summary>
		public static string MsgMenuBatchTrackTags="&Batch edit tags"; 
					
		/// <summary>
		/// &amp;Close FarPod panel
		/// </summary>
		public static string MsgMenuClosePanel="&Close FarPod panel"; 
					
		/// <summary>
		/// &amp;Eject selected device
		/// </summary>
		public static string MsgMenuEjectDevice="&Eject selected device"; 
					
		/// <summary>
		/// {0} {1} playlist(s) to:
		/// </summary>
		public static string MsgCopyPlayLists="{0} {1} playlist(s) to:"; 
					
		/// <summary>
		/// {0} {1} track(s) to:
		/// </summary>
		public static string MsgCopyTracks="{0} {1} track(s) to:"; 
					
		/// <summary>
		/// Delete {0} playlist(s) ?
		/// </summary>
		public static string MsgDeletePlayLists="Delete {0} playlist(s) ?"; 
					
		/// <summary>
		/// Delete {0} track(s) ?
		/// </summary>
		public static string MsgDeleteTracks="Delete {0} track(s) ?"; 
					
		/// <summary>
		/// {0} {1} files/dirs to playList:
		/// </summary>
		public static string MsgCopyFiles="{0} {1} files/dirs to playList:"; 
					
		/// <summary>
		/// Enter new playlist name:
		/// </summary>
		public static string MsgNewPlayList="Enter new playlist name:"; 
					
		/// <summary>
		/// Enter new file name:
		/// </summary>
		public static string MsgNewFileList="Enter new file name:"; 
					
		/// <summary>
		/// Rename playlist to:
		/// </summary>
		public static string MsgRenamePlayList="Rename playlist to:"; 
					
		/// <summary>
		/// Copy playlist to:
		/// </summary>
		public static string MsgLocalCopyPlayList="Copy playlist to:"; 
					
		/// <summary>
		/// Save file tags ...
		/// </summary>
		public static string MsgSaveFileTags="Save file tags ..."; 
					
		/// <summary>
		/// Save ipod database ...
		/// </summary>
		public static string MsgSaveIPodDB="Save ipod database ..."; 
					
		/// <summary>
		/// Direct operation only for one device!
		/// </summary>
		public static string MsgDirectOperationOnDiffDeviceWarning="Direct operation only for one device!"; 
				
		#endregion

        private static bool _isLoaded = false;
        private static readonly object _lockObject = new object();

        public static void Load(IModuleManager mm)
        {        
			if (!_isLoaded)
            {    
				lock (_lockObject)
				{
					if (!_isLoaded)
					{
						foreach (FieldInfo fi in typeof(MsgStr).GetFields(
							BindingFlags.Static | BindingFlags.Public))
						{
							fi.SetValue(null, mm.GetString(fi.Name));
						}

						_isLoaded = true;
					}
				}
			}
        }
    }
}

#pragma warning restore 0649
