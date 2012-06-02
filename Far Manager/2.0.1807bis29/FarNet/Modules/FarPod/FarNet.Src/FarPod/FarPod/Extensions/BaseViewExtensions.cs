namespace FarPod.Extensions
{
    using System;
    using FarPod.Dialogs.Bases;

    /// <summary>
    /// Generic BaseView Extensions
    /// </summary>
    static class BaseViewExtensions
    {
        public static void OnInit<T>(this T obj, Action<T> task) where T : BaseView
        {
            obj.Initializer = (d) => { task((T)d); };
        }
    }
}
