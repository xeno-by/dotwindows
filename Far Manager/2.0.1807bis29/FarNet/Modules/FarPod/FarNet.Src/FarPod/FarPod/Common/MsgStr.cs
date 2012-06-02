using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet;
using System.Reflection;

#pragma warning disable 0649

namespace FarPod.Common
{

    static class MsgStr
    {
        // Common
        public static string FarPod;
        public static string FarPodPrefix;        
        public static string GB;
        public static string MB;
        public static string Unknown;

        // Buttons
        public static string BtnOk;
        public static string BtnMove;
        public static string BtnCopy;
        public static string BtnDelete;
        public static string BtnDeleteWithTracks;
        public static string BtnDeleteFromPlaylist;
        public static string BtnCreate;
        public static string BtnCancel;
        public static string BtnSkip;
        public static string BtnSkipAll;
        public static string BtnReplace;
        public static string BtnReplaceAll;
        public static string BtnRetry;
        public static string BtnRename;
        public static string BtnAddExistToPlayList;        

        // Messages
        public static string MsgGettingDevices;
        public static string MsgGettingFiles;
        public static string MsgNoDirectCopyToDiffDevice;
        public static string MsgCopyOrMoveToPath;
        public static string MsgCopyOrMoveToDevice;
        public static string MsgDelete;
        public static string MsgCancel;
        public static string MsgOutputFormat;
        public static string MsgTrackExist;
        public static string MsgFileExist;
        public static string MsgCopierSource;
        public static string MsgCopierDestination;
        public static string MsgRememberChoice;

        // Dialog Messages
        public static string MsgCopyPlayLists;
        public static string MsgCopyTracks;
        public static string MsgCopyFiles;
        public static string MsgDeletePlayLists;
        public static string MsgDeleteTracks;
        public static string MsgNewPlayList;
        public static string MsgRenamePlayList;
        public static string MsgLocalCopyPlayList;

        private static bool _isLoaded = false;
        private static readonly object _lockObject = new object();

        public static void Load(IModuleManager mm)
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

#pragma warning restore 0649
