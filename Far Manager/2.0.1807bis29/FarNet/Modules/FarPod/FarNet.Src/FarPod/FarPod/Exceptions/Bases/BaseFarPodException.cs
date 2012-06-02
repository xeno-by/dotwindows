namespace FarPod.Exceptions.Bases
{
    using System;
    using FarNet;

    /// <summary>
    /// Base module exception 
    /// </summary>
    class BaseFarPodException : ModuleException
    {
        public BaseFarPodException(string message) : base(message) { }

        public BaseFarPodException(string message, Exception innerException) : base(message, innerException) { }
    }
}
