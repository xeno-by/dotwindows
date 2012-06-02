namespace FarPod.DataTypes
{
    using System;

    /// <summary>
    /// Store result of operation
    /// </summary>
    class OperationResult
    {
        public bool IsFailed { get; private set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public object ResultData { get; set; }

        public OperationResult(bool isFailed)
        {
            IsFailed = isFailed;
        }

        public OperationResult(Exception e)
        {
            IsFailed = true;
            Message = e.Message;
            Exception = e;
        }

        public OperationResult SetResult(object v)
        {
            ResultData = v;

            return this;
        }

        public OperationResult SetError(Exception e)
        {
            Exception = e;

            return this;
        }

        public OperationResult SetMessage(string msg)
        {
            Message = msg;

            return this;
        }
    }
}
