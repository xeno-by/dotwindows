namespace FarPod.Services
{
    using System;
    using System.Collections.Generic;
    using FarNet;
    using FarPod.Helpers;
    using FarPod.Resources;
    using SharePodLib;

    /// <summary>
    /// Main device service
    /// </summary>
    class FarPodDeviceService : IDisposable
    {
        private List<IPod> _deviceList;

        //void Device_IPodDisconnected(EventArgs args)
        //{
        //    if (args is StandardIPodDisconnectEventArgs)
        //    {
        //        StandardIPodDisconnectEventArgs e = (StandardIPodDisconnectEventArgs)args;

        //        IPod d = _deviceList.FirstOrDefault(p => p.FileSystem.DriveLetter == e.DriveLetter);

        //        if (d != null)
        //        {
        //            _deviceList.Remove(d);

        //            fireDeviceListChanged();
        //        }
        //    }
        //}

        //void Device_IPodConnected(IPod iPod)
        //{
        //    _deviceList.Add(iPod);

        //    fireDeviceListChanged();
        //}

        private void loadDeviceList()
        {
            if (_deviceList == null)
            {
                _deviceList = new List<IPod>();

                ProgressFormHelper.Invoke((f) =>
                {
                    _deviceList = IPod.GetAllConnectedIPods(IPodLoadAction.NoSync);
                },
                MsgStr.MsgGettingDevices);

                // remove iphone device.
                _deviceList.RemoveAll(p => (int)p.DeviceInfo.Family > 1000);
            }
        }

        public event EventHandler DeviceListChanged;

        private void fireDeviceListChanged()
        {
            if (Far.Net.Panel is FarPodPanel)
            {
                ((FarPodPanel)Far.Net.Panel).ProcessUpdatePanel();
            }

            if (Far.Net.Panel2 is FarPodPanel)
            {
                ((FarPodPanel)Far.Net.Panel2).ProcessUpdatePanel();
            }

            if (DeviceListChanged != null)
            {
                DeviceListChanged(null, EventArgs.Empty);
            }
        }

        public IList<IPod> GetDevices()
        {
            loadDeviceList();

            return _deviceList;
        }

        public void Refresh()
        {
            _deviceList = null;

            loadDeviceList();

            fireDeviceListChanged();
        }

        public void ClearAndFree()
        {
            _deviceList = null;
        }

        public void Eject(IPod dev)
        {
            ProgressFormHelper.Invoke(
                f => dev.Eject(),
                MsgStr.MsgEjecting);

            if (_deviceList.Contains(dev))
            {
                _deviceList.Remove(dev);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Device.IPodConnected -= Device_IPodConnected;
            //Device.IPodDisconnected -= Device_IPodDisconnected;

            //Device.StopListeningForDeviceChanges();            
        }

        #endregion
    }
}
