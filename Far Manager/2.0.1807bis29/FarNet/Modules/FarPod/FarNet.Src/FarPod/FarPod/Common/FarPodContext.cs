namespace FarPod.Common
{
    using FarNet;
    using FarPod.Services;

    /// <summary>
    /// Global Context, store ref to IModuleManager, FarPodDeviceService
    /// </summary>
    class FarPodContext
    {
        public static FarPodContext Current { get; private set; }

        public static void Init(IModuleManager mm)
        {
            if (Current == null)
            {
                Current = new FarPodContext(mm);
            }
        }

        public IModuleManager ModuleManager { get; private set; }

        public FarPodDeviceService DeviceSource { get; private set; }

        protected FarPodContext(IModuleManager mm)
        {
            DeviceSource = new FarPodDeviceService();

            ModuleManager = mm;            
        }
    }
}
