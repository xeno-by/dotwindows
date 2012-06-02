namespace FarPod.Helpers
{
    using System;
    using System.Threading;
    using FarNet.Tools;
    using FarPod.Exceptions;
    using FarPod.Resources;

    /// <summary>
    /// ProgressForm wrapper
    /// </summary>
    static class ProgressFormHelper
    {
        public static Exception LastError { get; set; }

        public static bool Invoke(Action<ProgressForm> task, string defMessage, bool silent = false, bool canCancel = true)
        {
            LastError = null;

            var pf = new ProgressForm();

            pf.CanCancel = canCancel;
            pf.Title = MsgStr.FarPod;
            pf.Activity = defMessage;

            Action taskWrapper = () => task(pf);

            LastError = pf.Invoke(new ThreadStart(taskWrapper));

            if (LastError != null && !silent)
            {                
                throw new AsyncFarPodException(LastError);
            }

            return pf.IsCompleted;            
        }        
    }
}
