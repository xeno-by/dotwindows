namespace FarPod.Exceptions
{
    using System;
    using FarPod.Exceptions.Bases;

    /// <summary>
    /// Operation exception
    /// </summary>
    class OperationFarPodException : BaseFarPodException
    {
        public OperationFarPodException(string message) : base(message) { }

        public OperationFarPodException(string message, Exception innerException) : base(message, innerException) { }

        public object AdditionalObjectInfo { get; set; }
    }
}
