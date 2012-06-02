namespace FarPod
{
    using System.IO;
    using System.Reflection;
    using FarNet;
    using FarPod.Resources;

    /// <summary>
    /// Host class
    /// </summary>
    [ModuleHost(Load = false)]
    public class FarPodHost : ModuleHost
    {
        public override void Connect()
        {
            MsgStr.Load(Manager);

            // Hack to remove all non ipod device
            SharePodLib.SharePodLib.RegisteredFileSystems.RemoveAll(p => p.Name != "IPod");

            // Setting this to valid values will disable the loading screen.            
            loadLicence();

            // Start up logging 
            SharePodLib.DebugLogger.StartLogging(getLocalPath("FarPod.log"));

            // EnableBackups not implemented ( ??? )
            SharePodLib.IPodBackup.EnableBackups = false;
        }

        private static void loadLicence()
        {
            if (File.Exists(getLocalPath("FarPod.lic")))
            {
                string[] lines = File.ReadAllLines(getLocalPath("FarPod.lic"));

                if (lines.Length == 2) SharePodLib.SharePodLib.SetLicence(lines[0], lines[1]);
            }
        }

        private static string getLocalPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        }
    }
}
