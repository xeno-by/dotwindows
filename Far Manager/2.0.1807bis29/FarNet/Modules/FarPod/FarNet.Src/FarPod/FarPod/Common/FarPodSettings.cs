using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FarNet;

namespace FarPod.Common
{
    static class FarPodSettings
    {
        // PlayLists
        public static string PlayListNameFormat = "{0}";
        public static string NewPlayListName = "NewPlayList";

        // Tracks
        public static string SupportFileMask = "*.mp3;*.m4a;*.m4v;*.wav;*.mp4;*.aac;*.m4b;*.aif;*.afc;*.m4r";
        public static string SupportArtWorkMask = "folder.jpg;cover.jpg;front.*jpg";
        public static string TrackNameFormatForPanel = "[Artist]-[Album] - [TrackNumber] [Title]";
        public static string TrackNameFormatForCopy = "[Artist]-[Year]-[Album]\\[TrackNumber]-[Title]";

        private static bool _isLoaded = false;
        private static readonly object _lockObject = new object();

        public static void Load(IModuleManager mm)
        {
            lock (_lockObject)
            {
                if (!_isLoaded)
                {
                    using (IRegistryKey rk = mm.OpenRegistryKey(null, false))
                    {
                        if (rk != null)
                        {
                            foreach (FieldInfo fi in typeof(FarPodSettings).GetFields(
                                BindingFlags.Static | BindingFlags.Public))
                            {
                                fi.SetValue(null, rk.GetValue(fi.Name, fi.GetValue(null)));
                            }
                        }
                    }

                    _isLoaded = true;
                }
            }
        }

        public static void Save(IModuleManager mm)
        {
            lock (_lockObject)
            {
                if (_isLoaded)
                {
                    using (IRegistryKey rk = mm.OpenRegistryKey(null, true))
                    {
                        foreach (FieldInfo fi in typeof(FarPodSettings).GetFields(
                            BindingFlags.Static | BindingFlags.Public))
                        {
                            rk.SetValue(fi.Name, fi.GetValue(null));
                        }
                    }

                    _isLoaded = false;
                }
            }
        }
    }
}
