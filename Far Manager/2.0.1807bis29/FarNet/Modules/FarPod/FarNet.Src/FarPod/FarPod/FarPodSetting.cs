namespace FarPod
{
    using System.Configuration;
    using FarNet.Settings;

    /// <summary>
    /// Settings
    /// </summary>
    [SettingsProvider(typeof(ModuleSettingsProvider))]
    public class FarPodSetting : ModuleSettings
    {
        /// <summary>
        /// The only settings instance.
        /// Normally settings are created once, when needed.
        /// </summary>        
        static readonly FarPodSetting _default = new FarPodSetting();

        /// <summary>
        /// Gets the public access to the settings instance.
        /// It is used for example by the core in order to open the settings panel.
        /// </summary>
        public static FarPodSetting Default { get { return _default; } }

        // PlayLists
        [UserScopedSetting]
        [DefaultSettingValue("{0}")]
        public string PlayListNameFormat
        {
            get { return (string)this["PlayListNameFormat"]; }
            set { this["PlayListNameFormat"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("NewPlayList")]
        public string NewPlayListName
        {
            get { return (string)this["NewPlayListName"]; }
            set { this["NewPlayListName"] = value; }
        }

        // Track
        [UserScopedSetting]
        [DefaultSettingValue("*.mp3;*.m4a;*.m4v;*.wav;*.mp4;*.aac;*.m4b;*.aif;*.afc;*.m4r")]
        public string SupportFileMask
        {
            get { return (string)this["SupportFileMask"]; }
            set { this["SupportFileMask"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("folder.jpg;cover.jpg;front.*jpg")]        
        public string SupportArtWorkMask
        {
            get { return (string)this["SupportArtWorkMask"]; }
            set { this["SupportArtWorkMask"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("[Artist]-[Album] - [TrackNumber] [Title]")]        
        public string TrackNameFormatForPanel
        {
            get { return (string)this["TrackNameFormatForPanel"]; }
            set { this["TrackNameFormatForPanel"] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("[Artist]-[Year]-[Album]\\[TrackNumber]-[Title]")]
        public string TrackNameFormatForCopy
        {
            get { return (string)this["TrackNameFormatForCopy"]; }
            set { this["TrackNameFormatForCopy"] = value; }
        }

        protected FarPodSetting()
        {
            
        }
    }
}
