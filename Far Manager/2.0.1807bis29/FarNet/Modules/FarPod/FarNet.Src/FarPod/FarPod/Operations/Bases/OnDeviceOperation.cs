namespace FarPod.Operations.Bases
{
    using System.Collections.Generic;
    using FarPod.DataTypes;
    using SharePodLib;
    using SharePodLib.Exceptions;

    /// <summary>
    /// Operation with device support
    /// </summary>
    abstract class OnDeviceOperation : BaseOperation
    {
        protected IPod device;

        protected bool isWriteMode;

        protected List<object> notProcessedItems;

        public IEnumerable<object> NotProcessedItems
        {
            get { return notProcessedItems; }
        }

        public OperationResult OperationResult
        {
            get;
            protected set;
        }

        protected OnDeviceOperation(IPod dev)
        {
            device = dev;

            isWriteMode = false;

            notProcessedItems = new List<object>();
        }

        protected override bool work()
        {
            OperationResult = deviceTransaction();

            return !OperationResult.IsFailed;
        }

        protected override void finish()
        {
            base.finish();

            if (IsCanceled && !IsCompleted && OperationResult == null)
            {
                OperationResult = new OperationResult(true);
            }
        }        

        private OperationResult deviceTransaction()
        {
            var result = new OperationResult(false);
            var taskResult = false;

            try
            {
                if (isWriteMode)
                {
                    device.AssertIsWritable();

                    device.AcquireLock();
                }

                taskResult = workOnDevice();

                if (isWriteMode)
                {
                    device.SaveChanges();
                }
            }
            catch (BaseSharePodLibException e)
            {
                result = new OperationResult(e);
            }            
            finally
            {
                if (isWriteMode)
                {
                    device.ReleaseLock();
                }
            }

            // ???
            if (isWriteMode)                
                device.Refresh();

            if (!taskResult)
                result = new OperationResult(true);

            return result;
        }

        protected abstract bool workOnDevice();
    }
}
