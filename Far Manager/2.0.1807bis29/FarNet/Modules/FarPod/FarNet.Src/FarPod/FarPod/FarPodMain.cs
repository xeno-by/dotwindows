namespace FarPod
{
    using System.Runtime.InteropServices;
    using FarNet;
    using FarPod.Common;
    using FarPod.Explorers;

    /// <summary>
    /// Main entry point to module
    /// </summary>
    [Guid(GuidContants.FarPodMainGuid)]
    [ModuleTool(Name = "FarPod", Options = ModuleToolOptions.Panels | ModuleToolOptions.Disk, Resources = true)]
    public class FarPodMain : ModuleTool
    {
        public override void Invoke(object sender, ModuleToolEventArgs e)
        {
            FarPodContext.Init(Manager);

            new DeviceExplorer().OpenPanel();
        }
    }    
}
