namespace FarPod.Exceptions
{
    using System;
    using FarPod.Exceptions.Bases;

    /// <summary>
    /// Async operation exception
    /// </summary>
    class AsyncFarPodException : BaseFarPodException
    {
        public const string ERROR_MESSAGE = "Error in acync operation!";

        public AsyncFarPodException() : base(ERROR_MESSAGE) { }

        public AsyncFarPodException(Exception innerException) : base(ERROR_MESSAGE, innerException) { }
    }
}
