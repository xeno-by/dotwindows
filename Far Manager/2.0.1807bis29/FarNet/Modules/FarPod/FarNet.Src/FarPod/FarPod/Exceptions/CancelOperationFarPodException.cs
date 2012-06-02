namespace FarPod.Exceptions
{
    using System;

    /// <summary>
    /// Cancel operation exception
    /// </summary>
    class CancelOperationFarPodException : OperationFarPodException
    {
        public const string ERROR_MESSAGE = "Operation was canceled by user!";

        public CancelOperationFarPodException() : base(ERROR_MESSAGE) { }

        public CancelOperationFarPodException(Exception innerException) : base(ERROR_MESSAGE, innerException) { }
    }
}
